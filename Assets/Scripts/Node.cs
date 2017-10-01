using System;

public class Node : IEquatable<Node>
{
    public Node(GridPosition position)
    {
        Position = position;
    }

    public GridPosition Position { get; }

    public float Fscore { get; set; }

    public float Gscore { get; set; }

    private static bool Equals(Node a, Node b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }
        return !ReferenceEquals(a, null) && a.Equals(b);
    }

    public bool Equals(Node other)
    {
        if (ReferenceEquals(other, null))
        {
            return false;
        }
        return Position.Equals(other.Position);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Node);
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }

    public static bool operator ==(Node left, Node right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Node left, Node right)
    {
        return !Equals(left, right);
    }

}