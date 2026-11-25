using System;
using System.Collections.Generic;

namespace Novent.Core.Rng
{
    /// <summary>
    /// Deterministic random stream. The host picks the world seed; every system forks its own
    /// stream from it, so all machines see the same map, spawns and loot, and one system drawing
    /// more numbers does not shift another system's results.
    /// </summary>
    public sealed class SeededRandom
    {
        private readonly System.Random _random;

        public SeededRandom(int seed)
        {
            Seed = seed;
            _random = new System.Random(seed);
        }

        public int Seed { get; }

        /// <summary>A child stream for one system, e.g. Fork("driftlife-spawns").</summary>
        public SeededRandom Fork(string streamName)
        {
            return new SeededRandom(unchecked(Seed * 31 + StableHash(streamName)));
        }

        /// <summary>Integer in [minInclusive, maxExclusive).</summary>
        public int Range(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }

        /// <summary>Float in [minInclusive, maxInclusive].</summary>
        public float Range(float minInclusive, float maxInclusive)
        {
            return minInclusive + (float)_random.NextDouble() * (maxInclusive - minInclusive);
        }

        public float Value01()
        {
            return (float)_random.NextDouble();
        }

        public bool Chance(float probability)
        {
            return _random.NextDouble() < probability;
        }

        public T Pick<T>(IReadOnlyList<T> items)
        {
            if (items == null || items.Count == 0)
            {
                throw new ArgumentException("Cannot pick from an empty list.", nameof(items));
            }
            return items[_random.Next(items.Count)];
        }

        // string.GetHashCode is randomised per process on .NET Core runtimes, so it would give
        // each machine a different stream; FNV-1a is stable everywhere.
        private static int StableHash(string text)
        {
            unchecked
            {
                var hash = (int)2166136261;
                foreach (var c in text)
                {
                    hash = (hash ^ c) * 16777619;
                }
                return hash;
            }
        }
    }
}
