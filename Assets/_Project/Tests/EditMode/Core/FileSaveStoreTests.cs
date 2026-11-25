using System;
using System.IO;
using Novent.Core.Save;
using NUnit.Framework;

namespace Novent.Tests.Core
{
    public class FileSaveStoreTests
    {
        private string _folder;
        private FileSaveStore _store;

        [SetUp]
        public void SetUp()
        {
            _folder = Path.Combine(Path.GetTempPath(), "novent-save-tests-" + Guid.NewGuid().ToString("N"));
            _store = new FileSaveStore(_folder);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, true);
            }
        }

        [Test]
        public void Write_ThenRead_ReturnsSameText()
        {
            _store.Write("world-1", "{\"version\":1}");

            Assert.That(_store.TryRead("world-1", out var json), Is.True);
            Assert.That(json, Is.EqualTo("{\"version\":1}"));
        }

        [Test]
        public void Write_ExistingSlot_ReplacesContentAndLeavesNoTempFile()
        {
            _store.Write("world-1", "old");

            _store.Write("world-1", "new");

            Assert.That(_store.TryRead("world-1", out var json), Is.True);
            Assert.That(json, Is.EqualTo("new"));
            Assert.That(Directory.GetFiles(_folder, "*.tmp"), Is.Empty);
        }

        [Test]
        public void TryRead_MissingSlot_ReturnsFalse()
        {
            Assert.That(_store.TryRead("nothing-here", out _), Is.False);
        }

        [Test]
        public void Delete_RemovesSlot()
        {
            _store.Write("world-1", "x");

            _store.Delete("world-1");

            Assert.That(_store.TryRead("world-1", out _), Is.False);
        }
    }
}
