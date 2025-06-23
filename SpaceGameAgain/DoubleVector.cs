using System.Diagnostics.CodeAnalysis;

namespace SpaceGame;

public struct DoubleVector : IEquatable<DoubleVector>
{
    public double X;
    public double Y;

    public static DoubleVector Zero => default;

    public DoubleVector(double x, double y)
    {
        X = x;
        Y = y;
    }

    public static DoubleVector FromVector2(Vector2 vector)
    {
        return new(vector.X, vector.Y);

    }

    public static DoubleVector FromVector2(float x, float y)
    {
        return new(x, y);
    }

    internal static DoubleVector Lerp(DoubleVector a, DoubleVector b, double t)
    {
        return new(double.Lerp(a.X, b.X, t), double.Lerp(a.Y, b.Y, t));
    }

    public Vector2 ToVector2()
    {
        return new((float)X, (float)Y);
    }

    public double Length()
    {
        return double.Sqrt(X * X + Y * Y);
    }

    public double LengthSquared()
    {
        return X * X + Y * Y;
    }

    public static DoubleVector Step(DoubleVector point, DoubleVector target, float distance)
    {
        DoubleVector vector = target - point;
        if (vector.LengthSquared() <= distance * distance)
        {
            return target;
        }

        return point + distance * vector.Normalized();
    }

    public static double Dot(DoubleVector a, DoubleVector b)
    {
        return a.X * b.X + a.Y * b.Y;
    }

    public static double Cross(DoubleVector a, DoubleVector b)
    {
        return a.X * b.Y - a.Y * b.X;
    }

    public DoubleVector Normalized()
    {
        return this * (1.0 / Length());
    }

    public static double Distance(DoubleVector a, DoubleVector b)
    {
        return (a - b).Length();
    }

    public static double DistanceSquared(DoubleVector a, DoubleVector b)
    {
        return (a - b).LengthSquared();
    }

    public static DoubleVector operator+(DoubleVector a, DoubleVector b)
    {
        return new(
            a.X + b.X,
            a.Y + b.Y
            );
    }

    public static DoubleVector operator -(DoubleVector a, DoubleVector b)
    {
        return new(
            a.X - b.X,
            a.Y - b.Y
            );
    }

    public static DoubleVector operator -(DoubleVector vector)
    {
        return new(
            -vector.X,
            -vector.Y
            );
    }

    public static DoubleVector operator *(DoubleVector a, double b)
    {
        return new(
            a.X * b,
            a.Y * b
            );
    }

    public static DoubleVector operator *(double a, DoubleVector b)
    {
        return new(
            a * b.X,
            a * b.Y
            );
    }

    public static DoubleVector operator /(DoubleVector a, double b)
    {
        return new(
            a.X / b,
            a.Y / b
            );
    }

    public static bool operator ==(DoubleVector a, DoubleVector b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(DoubleVector a, DoubleVector b)
    {
        return !a.Equals(b);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"<{X}, {Y}>";
    }

    public bool Equals(DoubleVector other)
    {
        return other.X == this.X && other.Y == this.Y;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is DoubleVector v && this.Equals(v);
    }
}
