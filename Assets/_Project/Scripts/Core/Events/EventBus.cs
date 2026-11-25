using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novent.Core.Events
{
    /// <summary>
    /// Local, typed publish/subscribe for "something happened" messages. One machine only —
    /// it does not cross the network. Events are readonly structs named ...Event.
    /// </summary>
    public static class EventBus<T> where T : struct
    {
        private static readonly List<Action<T>> s_handlers = new();
        private static Action<T>[] s_snapshot = Array.Empty<Action<T>>();
        private static bool s_isDirty;

        static EventBus()
        {
            EventBusRegistry.Track(Clear);
        }

        public static int SubscriberCount => s_handlers.Count;

        public static void Subscribe(Action<T> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            // A second subscribe almost always means OnEnable ran twice without OnDisable;
            // ignoring it keeps the handler from firing twice per event.
            if (s_handlers.Contains(handler))
            {
                Debug.LogWarning($"{handler.Method.Name} subscribed twice to {typeof(T).Name}; ignored.");
                return;
            }
            s_handlers.Add(handler);
            s_isDirty = true;
        }

        public static void Unsubscribe(Action<T> handler)
        {
            if (handler != null && s_handlers.Remove(handler))
            {
                s_isDirty = true;
            }
        }

        public static void Publish(T evt)
        {
            // Handlers run from a snapshot so the list may change mid-publish; the array is rebuilt
            // only when the subscriber list changed, so publishing does not allocate. Handlers added
            // during a publish receive the next event, not this one.
            if (s_isDirty)
            {
                s_snapshot = s_handlers.ToArray();
                s_isDirty = false;
            }
            var handlers = s_snapshot;
            for (var i = 0; i < handlers.Length; i++)
            {
                // A handler removed earlier in this same publish (its object was destroyed) must not run.
                if (s_isDirty && !s_handlers.Contains(handlers[i]))
                {
                    continue;
                }
                try
                {
                    handlers[i](evt);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        public static void Clear()
        {
            s_handlers.Clear();
            s_snapshot = Array.Empty<Action<T>>();
            // Dirty, so a publish already running skips the handlers it still holds in its snapshot.
            s_isDirty = true;
        }
    }

    /// <summary>Clears every event bus that has been used, e.g. between tests or plays.</summary>
    public static class EventBusRegistry
    {
        private static readonly List<Action> s_clearActions = new();

        internal static void Track(Action clear)
        {
            s_clearActions.Add(clear);
        }

        public static void ClearAll()
        {
            for (var i = 0; i < s_clearActions.Count; i++)
            {
                s_clearActions[i]();
            }
        }

        // With domain reload disabled, handlers from destroyed objects would otherwise survive into the next play.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearOnEnterPlayMode()
        {
            ClearAll();
        }
    }
}
