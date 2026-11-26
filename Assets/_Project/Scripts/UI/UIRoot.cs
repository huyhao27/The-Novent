using Novent.Core;
using UnityEngine;

namespace Novent.UI
{
    /// <summary>
    /// Put on the scene's main Canvas. Owns the PanelService for that scene and registers it,
    /// so gameplay and input code reach panels through Services.Get&lt;PanelService&gt;().
    /// </summary>
    // Registers before default-order scripts run Awake, so they can cache the PanelService there.
    [DefaultExecutionOrder(-100)]
    public sealed class UIRoot : MonoBehaviour
    {
        [SerializeField] private PanelRegistry _registry;

        [Tooltip("Where panel instances are created. Empty = this transform.")]
        [SerializeField] private Transform _panelParent;

        private PanelService _panels;

        private void Awake()
        {
            _panels = new PanelService(_registry, _panelParent != null ? _panelParent : transform);
            if (Services.TryGet<PanelService>(out _))
            {
                Debug.LogError($"Two UIRoots are active; {name} is ignored. Keep one per scene.", this);
                return;
            }
            Services.Register(_panels);
        }

        private void OnDestroy()
        {
            if (Services.TryGet<PanelService>(out var registered) && registered == _panels)
            {
                Services.Unregister<PanelService>();
            }
        }
    }
}
