using System;
using Novent.Core.Save;
using NUnit.Framework;

namespace Novent.Tests.Core
{
    public class SaveServiceTests
    {
        [Serializable]
        private sealed class StationSaveData
        {
            public int tally;
            public string[] repairedBays;
        }

        [Test]
        public void SaveThenLoad_RestoresSection()
        {
            var store = new MemorySaveStore();
            var writer = new SaveService(store);
            writer.Load("world-1");
            writer.Set("station", new StationSaveData { tally = 120, repairedBays = new[] { "workshop" } });
            writer.Save();

            var reader = new SaveService(store);
            var found = reader.Load("world-1");

            Assert.That(found, Is.True);
            Assert.That(reader.TryGet<StationSaveData>("station", out var data), Is.True);
            Assert.That(data.tally, Is.EqualTo(120));
            Assert.That(data.repairedBays, Is.EqualTo(new[] { "workshop" }));
        }

        [Test]
        public void Load_EmptySlot_ReturnsFalseAndHasNoSections()
        {
            var service = new SaveService(new MemorySaveStore());

            var found = service.Load("missing");

            Assert.That(found, Is.False);
            Assert.That(service.Has("station"), Is.False);
        }

        [Test]
        public void Load_NewerVersion_Throws()
        {
            var store = new MemorySaveStore();
            store.Write("world-1", "{\"version\":" + (SaveService.CurrentVersion + 1) + ",\"entries\":[]}");
            var service = new SaveService(store);

            Assert.Throws<InvalidOperationException>(() => service.Load("world-1"));
        }

        [Test]
        public void Save_BeforeLoad_Throws()
        {
            var service = new SaveService(new MemorySaveStore());

            Assert.Throws<InvalidOperationException>(() => service.Save());
        }
    }
}
