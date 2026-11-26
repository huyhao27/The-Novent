using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Novent.UI
{
    /// <summary>
    /// Opens panels by type, instantiating each prefab once on first use, and keeps them in a
    /// stack so Back closes the top one. Registered by UIRoot for the lifetime of its scene.
    /// </summary>
    public sealed class PanelService
    {
        private readonly PanelRegistry _registry;
        private readonly Transform _parent;
        private readonly Dictionary<Type, Panel> _instances = new();
        private readonly List<Panel> _stack = new();

        public PanelService(PanelRegistry registry, Transform parent)
        {
            _registry = registry;
            _parent = parent;
        }

        public Panel Top
        {
            get
            {
                PruneDestroyed();
                return _stack.Count > 0 ? _stack[^1] : null;
            }
        }

        public T Open<T>() where T : Panel
        {
            var panel = GetOrCreate<T>();
            _stack.Remove(panel);
            _stack.Add(panel);
            panel.transform.SetAsLastSibling();
            if (!panel.IsOpen)
            {
                panel.Open();
            }
            return panel;
        }

        public void Close<T>() where T : Panel
        {
            if (_instances.TryGetValue(typeof(T), out var panel))
            {
                Close(panel);
            }
        }

        public void Close(Panel panel)
        {
            if (panel == null || !panel.IsOpen)
            {
                return;
            }
            _stack.Remove(panel);
            panel.Close();
        }

        /// <summary>Closes the top panel if it allows it. Returns true when Back was consumed by the UI.</summary>
        public bool Back()
        {
            var top = Top;
            if (top == null)
            {
                return false;
            }
            if (top.CloseOnBack)
            {
                Close(top);
            }
            return true;
        }

        public bool IsOpen<T>() where T : Panel
        {
            return _instances.TryGetValue(typeof(T), out var panel) && panel != null && panel.IsOpen;
        }

        // A panel destroyed by something else (its scene object removed) must not block Back/Esc
        // or be returned from the cache; "== null" is Unity's destroyed-object check.
        private void PruneDestroyed()
        {
            _stack.RemoveAll(panel => panel == null);
        }

        private T GetOrCreate<T>() where T : Panel
        {
            if (_instances.TryGetValue(typeof(T), out var existing) && existing != null)
            {
                return (T)existing;
            }
            var prefab = _registry != null ? _registry.Find<T>() : null;
            if (prefab == null)
            {
                throw new InvalidOperationException(
                    $"Panel {typeof(T).Name} is not in the PanelRegistry assigned to UIRoot.");
            }
            var instance = Object.Instantiate(prefab, _parent);
            instance.HideImmediate();
            _instances[typeof(T)] = instance;
            return instance;
        }
    }
}
