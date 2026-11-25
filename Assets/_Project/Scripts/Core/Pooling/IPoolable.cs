namespace Novent.Core.Pooling
{
    /// <summary>
    /// Implemented by components on pooled objects that must reset state between uses.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
