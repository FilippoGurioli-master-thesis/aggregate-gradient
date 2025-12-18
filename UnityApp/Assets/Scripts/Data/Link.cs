using System;

namespace Data
{
    public sealed class Link : IEquatable<Link>
    {
        public int Node1 { get; }
        public int Node2 { get; }

        public Link(int node1, int node2)
        {
            // Normalize order so
            if (node1 <= node2)
            {
                Node1 = node1;
                Node2 = node2;
            }
            else
            {
                Node1 = node2;
                Node2 = node1;
            }
        }

        public bool Equals(Link other)
            => other is not null && Node1 == other.Node1 && Node2 == other.Node2;

        public override bool Equals(object obj)
            => obj is Link other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(Node1, Node2);
    }
}
