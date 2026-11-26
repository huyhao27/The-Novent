namespace Novent.Core.Save
{
    /// <summary>Where save text lives. FileSaveStore in the game, MemorySaveStore in tests.</summary>
    public interface ISaveStore
    {
        bool TryRead(string slot, out string json);
        void Write(string slot, string json);
        void Delete(string slot);
    }
}
