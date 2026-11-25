using System.Collections.Generic;

namespace Novent.Core.Save
{
    /// <summary>In-memory store for tests and for sessions that must not touch disk.</summary>
    public sealed class MemorySaveStore : ISaveStore
    {
        private readonly Dictionary<string, string> _slots = new();

        public bool TryRead(string slot, out string json)
        {
            return _slots.TryGetValue(slot, out json);
        }

        public void Write(string slot, string json)
        {
            _slots[slot] = json;
        }

        public void Delete(string slot)
        {
            _slots.Remove(slot);
        }
    }
}
