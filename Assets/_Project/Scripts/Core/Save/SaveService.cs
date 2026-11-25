using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novent.Core.Save
{
    /// <summary>
    /// One save slot made of keyed sections; each feature owns a [Serializable] section class.
    /// In co-op the host owns the world slot and every player keeps a profile slot.
    /// </summary>
    public sealed class SaveService
    {
        /// <summary>Bump when a saved shape changes, and add the step to Migrate.</summary>
        public const int CurrentVersion = 1;

        private readonly ISaveStore _store;
        private readonly Dictionary<string, string> _sections = new();

        public SaveService(ISaveStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string Slot { get; private set; }

        /// <summary>Opens a slot. Returns false when nothing was saved there yet (sections start empty).</summary>
        public bool Load(string slot)
        {
            Slot = slot;
            _sections.Clear();
            if (!_store.TryRead(slot, out var json) || string.IsNullOrEmpty(json))
            {
                return false;
            }
            var file = JsonUtility.FromJson<SaveFile>(json);
            if (file.version > CurrentVersion)
            {
                throw new InvalidOperationException(
                    $"Save '{slot}' is version {file.version}; this build reads up to {CurrentVersion}.");
            }
            Migrate(file);
            foreach (var entry in file.entries)
            {
                _sections[entry.key] = entry.json;
            }
            return true;
        }

        public void Save()
        {
            if (Slot == null)
            {
                throw new InvalidOperationException("Call Load(slot) before Save().");
            }
            var file = new SaveFile { version = CurrentVersion };
            foreach (var pair in _sections)
            {
                file.entries.Add(new SaveEntry { key = pair.Key, json = pair.Value });
            }
            _store.Write(Slot, JsonUtility.ToJson(file));
        }

        public void Set<T>(string key, T section)
        {
            _sections[key] = JsonUtility.ToJson(section);
        }

        public bool TryGet<T>(string key, out T section)
        {
            if (_sections.TryGetValue(key, out var json))
            {
                section = JsonUtility.FromJson<T>(json);
                return true;
            }
            section = default;
            return false;
        }

        public bool Has(string key)
        {
            return _sections.ContainsKey(key);
        }

        public void Remove(string key)
        {
            _sections.Remove(key);
        }

        private static void Migrate(SaveFile file)
        {
            // Add one block per version step (1 → 2, 2 → 3 …): it rewrites the entries whose shape
            // changed and bumps file.version, so an old save walks forward one step at a time.
            file.version = CurrentVersion;
        }

        [Serializable]
        private sealed class SaveFile
        {
            public int version;
            public List<SaveEntry> entries = new();
        }

        [Serializable]
        private sealed class SaveEntry
        {
            public string key;
            public string json;
        }
    }
}
