using System;
using System.Collections.Generic;
using UnityEngine;

public struct GridPosition : IEquatable<GridPosition>
{
    private readonly int _u;
    private readonly int _v;
    public const int Size = 1;
    private const float Sqrt32 = 0.86602540378443864676372317075294f;

    public GridPosition(int u, int v)
    {
        _u = u;
        _v = v;
    }

    public GridPosition(Vector3 position)
    {
        var yy = 1 / Sqrt32 * position.z / Size + 1;
        var xx = position.x / Size + yy / 2 + 0.5f;
        _u = Mathf.FloorToInt((Mathf.Floor(xx) + Mathf.Floor(yy)) / 3);
        _v = Mathf.FloorToInt((xx - yy + _u + 1) / 2);
    }

    public Vector3 GetPosition()
    {
        var x = Size * _v * 1.5f;
        var y = Size * (2 * _u - _v) * Sqrt32;
        return new Vector3(x, 0, y);
    }

    public GridPosition North
    {
        get { return new GridPosition(_u + 1, _v); }
    }

    public GridPosition NorthEast
    {
        get { return new GridPosition(_u + 1, _v + 1); }
    }

    public GridPosition SouthEast
    {
        get { return new GridPosition(_u, _v + 1); }
    }

    public GridPosition South
    {
        get { return new GridPosition(_u - 1, _v); }
    }

    public GridPosition SouthWest
    {
        get { return new GridPosition(_u - 1, _v - 1); }
    }

    public GridPosition NorthWest
    {
        get { return new GridPosition(_u, _v - 1); }
    }

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
        var diffU = other._u - _u;
        var diffV = other._v - _v;
        return Math.Max(Math.Max(Math.Abs(diffU), Math.Abs(diffV)), Math.Abs(diffU - diffV));
    }

    public bool Equals(GridPosition other)
    {
        return _u == other._u && _v == other._v;
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
            return (_u * 397) ^ _v;
        }
    }

    public override string ToString()
    {
        return string.Format("u:{0}, v:{1}", _u, _v);
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