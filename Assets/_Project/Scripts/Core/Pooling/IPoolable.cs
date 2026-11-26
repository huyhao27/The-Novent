namespace Novent.Core.Pooling
{
    /// <summary>
    /// Implemented by components on pooled objects that must reset state between uses.
    /// OnSpawned runs after the object is positioned and active; OnDespawned runs before it is
    /// stored, so reset state there rather than in OnEnable.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
