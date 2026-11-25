using System;

namespace Novent.Core.Session
{
    /// <summary>
    /// Game-side player identity, so netcode ids (e.g. a client id) never leak into gameplay code.
    /// </summary>
    public readonly struct PlayerId : IEquatable<PlayerId>
    {
        public static readonly PlayerId Local = new(0);

        public PlayerId(ulong value)
        {
            Value = value;
        }

        public ulong Value { get; }

        public bool Equals(PlayerId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"Player#{Value}";
        }

        public static bool operator ==(PlayerId left, PlayerId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerId left, PlayerId right)
        {
            return !left.Equals(right);
        }
    }
}
