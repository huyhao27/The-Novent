using System.IO;
using UnityEngine;

namespace Novent.Core.Save
{
    /// <summary>Saves to persistentDataPath/saves/&lt;slot&gt;.json.</summary>
    public sealed class FileSaveStore : ISaveStore
    {
        private readonly string _folder;

        public FileSaveStore() : this(Path.Combine(Application.persistentDataPath, "saves"))
        {
        }

        public FileSaveStore(string folder)
        {
            _folder = folder;
        }

        public bool TryRead(string slot, out string json)
        {
            var path = PathFor(slot);
            if (!File.Exists(path))
            {
                json = null;
                return false;
            }
            json = File.ReadAllText(path);
            return true;
        }

        public void Write(string slot, string json)
        {
            Directory.CreateDirectory(_folder);
            var path = PathFor(slot);
            // Write beside the old file and swap, so a crash mid-write never leaves half a save.
            var temp = path + ".tmp";
            File.WriteAllText(temp, json);
            if (File.Exists(path))
            {
                File.Replace(temp, path, null);
            }
            else
            {
                File.Move(temp, path);
            }
        }

        public void Delete(string slot)
        {
            var path = PathFor(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private string PathFor(string slot)
        {
            return Path.Combine(_folder, slot + ".json");
        }
    }
}
