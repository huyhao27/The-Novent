using Novent.Core.Rng;
using NUnit.Framework;

namespace Novent.Tests.Core
{
    public class SeededRandomTests
    {
        [Test]
        public void SameSeed_SameSequence()
        {
            var host = new SeededRandom(1234);
            var client = new SeededRandom(1234);

            for (var i = 0; i < 20; i++)
            {
                Assert.That(client.Range(0, 1000), Is.EqualTo(host.Range(0, 1000)));
            }
        }

        [Test]
        public void Fork_SameName_SameStreamOnEveryMachine()
        {
            var host = new SeededRandom(99).Fork("driftlife-spawns");
            var client = new SeededRandom(99).Fork("driftlife-spawns");

            Assert.That(client.Seed, Is.EqualTo(host.Seed));
            Assert.That(client.Range(0, 1000), Is.EqualTo(host.Range(0, 1000)));
        }

        [Test]
        public void Fork_DifferentNames_DifferentStreams()
        {
            var world = new SeededRandom(99);

            Assert.That(world.Fork("loot").Seed, Is.Not.EqualTo(world.Fork("map-chunks").Seed));
        }

        [Test]
        public void Range_Float_StaysInBounds()
        {
            var random = new SeededRandom(7);

            for (var i = 0; i < 200; i++)
            {
                Assert.That(random.Range(2f, 3f), Is.InRange(2f, 3f));
            }
        }
    }
}
