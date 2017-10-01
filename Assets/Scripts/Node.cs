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

    private static bool Equals(Node left, Node right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }
        return !ReferenceEquals(left, null) && left.Equals(right);
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