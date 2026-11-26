using System.Collections.Generic;
using UnityEngine;

namespace Novent.UI
{
    /// <summary>Every panel prefab the game can open. Adding a screen = add its prefab here.</summary>
    [CreateAssetMenu(menuName = "Novent/UI/Panel Registry", fileName = "PanelRegistry")]
    public sealed class PanelRegistry : ScriptableObject
    {
        [SerializeField] private List<Panel> _panels = new();

        public T Find<T>() where T : Panel
        {
            foreach (var panel in _panels)
            {
                // A missing prefab reference is "fake null": the type test alone would accept it.
                if (panel != null && panel is T match)
                {
                    return match;
                }
            }
            return null;
        }
    }
}
