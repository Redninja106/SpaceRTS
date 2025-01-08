using SimulationFramework.Drawing;
using SimulationFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace SpaceGame;

public struct Transform
{
    public FixedVector2 Position = FixedVector2.Zero;
    public float Rotation = 0;
    public Vector2 Scale = Vector2.One;

    public FixedVector2 Forward => FixedVector2.FromVector2(Angle.ToVector(Rotation));
    
    public static readonly Transform Default = new();

    public Transform()
    {
    }

    public Matrix3x2 CreateWorldToLocalMatrix()
    {
        return
            Matrix3x2.CreateTranslation(-Position.ToVector2()) *
            Matrix3x2.CreateRotation(-Rotation) *
            Matrix3x2.CreateScale(1f / Scale.X, 1f / Scale.Y);
    }

    public Matrix3x2 CreateLocalToWorldMatrix()
    {
        return
            Matrix3x2.CreateScale(Scale) *
            Matrix3x2.CreateRotation(Rotation) *
            Matrix3x2.CreateTranslation(Position.ToVector2());
    }

    public void Layout()
    {
        // ImGui.DragFloat2("Position", ref Position);
        ImGui.SliderAngle("Rotation", ref Rotation);
        ImGui.DragFloat2("Scale", ref Scale);
    }

    public void ApplyTo(ICanvas canvas)
    {
        canvas.Transform(this.CreateLocalToWorldMatrix());
    }

    public Vector2 WorldToLocal(Vector2 point)
    {
        return Vector2.Transform(point, CreateWorldToLocalMatrix());
    }

    public Vector2 LocalToWorld(Vector2 point)
    {
        return Vector2.Transform(point, CreateLocalToWorldMatrix());
    }

    public Transform Rotated(float angle)
    {
        return this with { Rotation = Rotation + angle };
    }

    public Transform Translated(FixedVector2 translation)
    {
        return this with { Position = Position + translation }; 
    }

    public Transform Scaled(float scale)
    {
        return this with { Scale = Scale * scale };
    }

    public float Distance(Transform transform)
    {
        return Vector2.Distance(this.Position.ToVector2(), transform.Position.ToVector2());
    }

    public static Transform Lerp(Transform a, Transform b, float t)
    {
        Transform result = default;
        result.Position = FixedVector2.Lerp(a.Position, b.Position, t);
        result.Rotation = Angle.Lerp(a.Rotation, b.Rotation, t);
        result.Scale = Vector2.Lerp(a.Scale, b.Scale, t);
        return result;
    }
}

public struct FixedVector2
{
    public Fixed32 X;
    public Fixed32 Y;

    public static FixedVector2 Zero => default;

    public FixedVector2(Fixed32 x, Fixed32 y)
    {
        X = x;
        Y = y;
    }

    public static FixedVector2 FromVector2(Vector2 vector)
    {
        return new(Fixed32.FromFloat(vector.X), Fixed32.FromFloat(vector.Y));
    }

    public static FixedVector2 FromVector2(float x, float y)
    {
        return new(Fixed32.FromFloat(x), Fixed32.FromFloat(y));
    }

    internal static FixedVector2 Lerp(FixedVector2 position1, FixedVector2 position2, float t)
    {
        return new(Fixed32.Lerp(position1.X, position2.X, t), Fixed32.Lerp(position1.Y, position2.Y, t));
    }

    public Vector2 ToVector2()
    {
        return new(X.ToFloat(), Y.ToFloat());
    }

    public float Length()
    {
        return this.ToVector2().Length();
    }
    public float LengthSquared()
    {
        return this.ToVector2().LengthSquared();
    }
    public FixedVector2 Normalized()
    {
        return this / this.Length();
    }

    public static float Distance(FixedVector2 a, FixedVector2 b)
    {
        return Vector2.Distance(a.ToVector2(), b.ToVector2());
    }

    public static FixedVector2 operator+(FixedVector2 a, FixedVector2 b)
    {
        return new(
            a.X + b.X,
            a.Y + b.Y
            );
    }

    public static FixedVector2 operator -(FixedVector2 a, FixedVector2 b)
    {
        return new(
            a.X - b.X,
            a.Y - b.Y
            );
    }

    public static FixedVector2 operator *(FixedVector2 a, float b)
    {
        return new(
            a.X * Fixed32.FromFloat(b),
            a.Y * Fixed32.FromFloat(b)
            );
    }

    public static FixedVector2 operator *(float a, FixedVector2 b)
    {
        return new(
            Fixed32.FromFloat(a) * b.X,
            Fixed32.FromFloat(a) * b.Y
            );
    }

    public static FixedVector2 operator /(FixedVector2 a, float b)
    {
        return new(
            a.X / Fixed32.FromFloat(b),
            a.Y / Fixed32.FromFloat(b) 
            );
    }
}

public struct Fixed32
{
    const int FRACTION_BITS = 11;
    const int FRACTION_MASK = 0x07FF;

    public int bits;

    public static Fixed32 Zero => default;

    public Fixed32(int bits)
    {
        this.bits = bits;
    }

    public static Fixed32 FromParts(int integer, float fraction)
    {
        return new(integer << FRACTION_BITS | (int)(fraction * FRACTION_MASK));
    }

    public static Fixed32 FromScientific(double value, int exponent)
    {
        throw new NotImplementedException();
    }

    public static Fixed32 FromDecimal(decimal value)
    {
        int whole = (int)value;
        int fraction = (int)((Math.Abs(value) - Math.Abs(whole)) * FRACTION_MASK);
        return new(whole << FRACTION_BITS | fraction);
    }

    public decimal ToDecimal()
    {
        return (bits / (decimal)FRACTION_MASK);
    }

    public double ToDouble()
    {
        return (bits / (double)FRACTION_MASK);
    }

    public float ToFloat()
    {
        return (bits / (float)FRACTION_MASK);
    }

    public static double DoubleDivide(Fixed32 a, Fixed32 b)
    {
        return a.ToDouble() / b.ToDouble();
    }

    public static Fixed32 operator +(Fixed32 a, Fixed32 b)
    {
        return new(a.bits + b.bits);
    }

    public static Fixed32 operator -(Fixed32 a)
    {
        return new(-a.bits);
    }

    public static Fixed32 operator -(Fixed32 a, Fixed32 b)
    {
        return new(a.bits - b.bits);
    }

    public static Fixed32 operator *(Fixed32 a, Fixed32 b)
    {
        return new((a.bits * b.bits) >> FRACTION_BITS);
    }

    public static Fixed32 operator /(Fixed32 a, Fixed32 b)
    {
        return new((a.bits / b.bits) >> FRACTION_BITS);
    }

    public override string ToString()
    {
        long whole = bits >> FRACTION_BITS;
        long frac = bits & FRACTION_MASK;
        return $"{whole}{frac / (double)FRACTION_MASK:.######}";

    }

    //public static void ImGuiInput(string label, ref Fixed32 value, bool exponential = true)
    //{
    //    ImGui.BeginGroup();
    //    ImGui.PushID(label);

    //    long wholeMin = 2 << 47, wholeMax = (2 << 47) - 1;

    //    long whole = value.bits >> FRACTION_BITS;
    //    float step = exponential ? MathF.Max(1, MathF.Abs(0.01f * whole)) : 1;
    //    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 3);
    //    // ImGui.DragScalar("##whole", ImGuiDataType.S64, (nint)(&whole), step, (nint)(&wholeMin), (nint)(&wholeMax));

    //    float fraction = (value.bits & FRACTION_MASK) / (float)FRACTION_MASK;
    //    ImGui.SameLine();
    //    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() / 3);
    //    ImGui.DragFloat("##fract", ref fraction, 0.001f, 0, 1, "%0.5f");

    //    value = FromParts(whole, fraction);

    //    ImGui.SameLine();
    //    ImGui.Text(label + $" ({value.ToDecimal():e})");

    //    ImGui.PopID();
    //    ImGui.EndGroup();
    //}

    public static Fixed32 FromDouble(double value)
    {
        int whole = (int)value;
        int fraction = (int)((Math.Abs(value) - Math.Abs(whole)) * FRACTION_MASK);
        return new(whole << FRACTION_BITS | fraction);
    }

    internal static Fixed32 Lerp(Fixed32 a, Fixed32 b, float t)
    {
        return Fixed32.FromDouble((double)float.Lerp(a.ToFloat(), b.ToFloat(), t));
    }

    public static Fixed32 FromFloat(float value)
    {
        int whole = (int)value;
        int fraction = (int)((MathF.Abs(value) - MathF.Abs(whole)) * FRACTION_MASK);
        return new(whole << FRACTION_BITS | fraction);
    }
}
