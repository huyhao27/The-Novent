using UnityEngine;

namespace Novent.UI
{
    /// <summary>
    /// Base for every screen and popup. Opened and closed only through PanelService, which keeps
    /// the stack used by Back/Esc. Subclasses put their open/close animation in OnOpened/OnClosed.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Panel : MonoBehaviour
    {
        [Tooltip("Back/Esc closes this panel. Off for panels that need an explicit choice.")]
        [SerializeField] private bool _closeOnBack = true;

        private CanvasGroup _canvasGroup;

        public bool IsOpen { get; private set; }

        public bool CloseOnBack => _closeOnBack;

        private CanvasGroup Group
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                }
                return _canvasGroup;
            }
        }

        internal void Open()
        {
            IsOpen = true;
            SetVisible(true);
            OnOpened();
        }

        internal void Close()
        {
            IsOpen = false;
            SetVisible(false);
            OnClosed();
        }

        internal void HideImmediate()
        {
            IsOpen = false;
            SetVisible(false);
        }

        protected virtual void OnOpened()
        {
        }

        protected virtual void OnClosed()
        {
        }

        private void SetVisible(bool visible)
        {
            Group.alpha = visible ? 1f : 0f;
            Group.interactable = visible;
            Group.blocksRaycasts = visible;
        }
    }
}
