using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novent.Core
{
    /// <summary>
    /// Registry of the game's shared services. Filled by GameBootstrap before the first scene
    /// loads; everything else only reads from it.
    /// </summary>
    public static class Services
    {
        private static readonly Dictionary<Type, object> s_services = new();

        /// <summary>True once GameBootstrap has registered every core service.</summary>
        public static bool IsReady { get; private set; }

        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }
            if (s_services.ContainsKey(typeof(T)))
            {
                throw new InvalidOperationException(
                    $"Service {typeof(T).Name} is already registered. Unregister the old one first.");
            }
            s_services[typeof(T)] = service;
        }

        public static void Unregister<T>() where T : class
        {
            s_services.Remove(typeof(T));
        }

        public static T Get<T>() where T : class
        {
            if (s_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException(
                $"Service {typeof(T).Name} is not registered. Register it in GameBootstrap, " +
                "or in the scene root that owns it.");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (s_services.TryGetValue(typeof(T), out var found))
            {
                service = (T)found;
                return true;
            }
            service = null;
            return false;
        }

        public static void MarkReady()
        {
            IsReady = true;
        }

        /// <summary>Disposes disposable services and empties the registry. Tests call it in TearDown.</summary>
        public static void Reset()
        {
            foreach (var service in s_services.Values)
            {
                if (service is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            s_services.Clear();
            IsReady = false;
        }

        // With domain reload disabled (Enter Play Mode Options) statics survive between plays.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnEnterPlayMode()
        {
            Reset();
        }
    }
}
