using SimulationFramework.Drawing.Shaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;

struct HexCoordinate : IEquatable<HexCoordinate>
{
    public int Q;
    public int R;
    public int S;

    public static readonly HexCoordinate UnitQ = new(1, 0);
    public static readonly HexCoordinate UnitR = new(0, 1);
    public static readonly HexCoordinate UnitS = new(1, 1);
    public static readonly HexCoordinate Zero = new(0, 0);

    public HexCoordinate(int q, int r)
    {
        Q = q;
        R = r;
        S = -R - Q;
    }

    public HexCoordinate(int q, int r, int s)
    {
        Debug.Assert(q + r + s == 0);
        Q = q;
        R = r;
        S = s;
    }

    public Vector2 ToCartesian()
    {
        float x = 3f / 2f * Q;
        float y = MathF.Sqrt(3.0f) / 2.0f * Q + MathF.Sqrt(3.0f) * R;
        return new Vector2(x, y);
    }

    public static HexCoordinate FromCartesian(Vector2 cartesian)
    {
        float q = 2f / 3f * cartesian.X;
        float r = -(1f / 3f) * cartesian.X + MathF.Sqrt(3f) / 3f * cartesian.Y;
        float s = -q - r;

        int qi = (int)MathF.Round(q);
        int ri = (int)MathF.Round(r);
        int si = (int)MathF.Round(s);
        float q_diff = MathF.Abs(qi - q);
        float r_diff = MathF.Abs(ri - r);
        float s_diff = MathF.Abs(si - s);

        if (And(q_diff > r_diff, q_diff > s_diff))
        {
            qi = -ri - si;
        }
        else if (r_diff > s_diff)
        {
            ri = -qi - si;
        }

        return new(qi, ri);
    }

    private static bool And(bool a, bool b)
    {
        return a && b;
    }

    public readonly bool Equals(HexCoordinate other)
    {
        return R == other.R && Q == other.Q && S == other.S;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is HexCoordinate b && Equals(b);
    }

    public static bool operator ==(HexCoordinate a, HexCoordinate b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(HexCoordinate a, HexCoordinate b)
    {
        return !a.Equals(b);
    }

    public static HexCoordinate operator -(HexCoordinate a)
    {
        return new(-a.Q, -a.R, -a.S);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Q, R, S);
    }

    public static HexCoordinate operator +(HexCoordinate a, HexCoordinate b)
    {
        return new(a.Q + b.Q, a.R + b.R);
    }

    public static HexCoordinate operator -(HexCoordinate a, HexCoordinate b)
    {
        return new(a.Q - b.Q, a.R - b.R);
    }

    public readonly HexCoordinate RotatedLeft()
    {
        return new(-this.S, -this.Q, -this.R);
    }

    public readonly HexCoordinate RotatedRight()
    {
        return new(-this.R, -this.S, -this.Q);
    }

    public readonly HexCoordinate Rotated(int rotation)
    {
        HexCoordinate result = this;
        while (rotation > 0)
        {
            result = result.RotatedRight();
            rotation--;
        }
        while (rotation < 0)
        {
            result = result.RotatedLeft();
            rotation++;
        }
        return result;
    }

    public override string ToString()
    {
        return $"<q: {Q} r: {R} s: {S}>";
    }
}