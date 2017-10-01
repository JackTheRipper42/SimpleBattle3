using System;
using System.Collections.Generic;
using UnityEngine;

public struct GridPosition : IEquatable<GridPosition>
{
    private const int Size = 1;
    private const float Sqrt32 = 0.86602540378443864676372317075294f;

    private GridPosition(int u, int v)
    {
        U = u;
        V = v;
    }

    private GridPosition(Vector3 position)
    {
        var yy = 1 / Sqrt32 * position.z / Size + 1;
        var xx = position.x / Size + yy / 2 + 0.5f;
        U = Mathf.FloorToInt((Mathf.Floor(xx) + Mathf.Floor(yy)) / 3);
        V = Mathf.FloorToInt((xx - yy + U + 1) / 2);
    }

    public int U { get; }

    public int V { get; }

    public static GridPosition FromVector3(Vector3 position)
    {
        return new GridPosition(position);
    }

    public static Vector3 ToVector3(GridPosition position)
    {
        return position.ToVector3();
    }

    private Vector3 ToVector3()
    {
        var x = Size * V * 1.5f;
        var y = Size * (2 * U - V) * Sqrt32;
        return new Vector3(x, 0, y);
    }

    public GridPosition North => new GridPosition(U + 1, V);

    public GridPosition NorthEast => new GridPosition(U + 1, V + 1);

    public GridPosition SouthEast => new GridPosition(U, V + 1);

    public GridPosition South => new GridPosition(U - 1, V);

    public GridPosition SouthWest => new GridPosition(U - 1, V - 1);

    public GridPosition NorthWest => new GridPosition(U, V - 1);

    public IEnumerable<GridPosition> Neighbors
    {
        get
        {
            yield return North;
            yield return NorthEast;
            yield return SouthEast;
            yield return South;
            yield return SouthWest;
            yield return NorthWest;
        }
    }

    public int Distance(GridPosition other)
    {
        var diffU = other.U - U;
        var diffV = other.V - V;
        return Math.Max(Math.Max(Math.Abs(diffU), Math.Abs(diffV)), Math.Abs(diffU - diffV));
    }

    public bool Equals(GridPosition other)
    {
        return U == other.U && V == other.V;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }
        return obj is GridPosition && Equals((GridPosition) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (U * 397) ^ V;
        }
    }

    public override string ToString()
    {
        return $"u:{U}, v:{V}";
    }

    public static bool operator ==(GridPosition left, GridPosition right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GridPosition left, GridPosition right)
    {
        return !left.Equals(right);
    }
}