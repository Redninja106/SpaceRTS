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
    public DoubleVector Position = DoubleVector.Zero;
    public float Rotation = 0;
    public Vector2 Scale = Vector2.One;

    public DoubleVector Forward => DoubleVector.FromVector2(Angle.ToVector(Rotation));
    
    public static readonly Transform Default = new();

    public Transform()
    {
    }

    public static Transform Create(DoubleVector position, float rotation)
    {
        return Default with
        {
            Position = position,
            Rotation = rotation,
        };
    }

    public Matrix3x2 WorldToLocalMatrix()
    {
        return
            Matrix3x2.CreateTranslation(-Position.ToVector2()) *
            Matrix3x2.CreateRotation(-Rotation) *
            Matrix3x2.CreateScale(1f / Scale.X, 1f / Scale.Y);
    }

    public Matrix3x2 LocalToWorldMatrix()
    {
        return
            Matrix3x2.CreateScale(Scale) *
            Matrix3x2.CreateRotation(Rotation) *
            Matrix3x2.CreateTranslation(Position.ToVector2());
      
    }

    public void Layout()
    {
        Vector2 pos = this.Position.ToVector2();
        if (ImGui.DragFloat2("Position", ref pos))
        {
            this.Position = DoubleVector.FromVector2(pos);
        }
        ImGui.SliderAngle("Rotation", ref this.Rotation);
        ImGui.DragFloat2("Scale", ref this.Scale);
    }

    public void ApplyTo(ICanvas canvas, Camera camera)
    {
        canvas.Transform(camera.CreateRelativeMatrix(this));
    }

    public Vector2 WorldToLocal(Vector2 point)
    {
        return Vector2.Transform(point, WorldToLocalMatrix());
    }

    public Vector2 LocalToWorld(Vector2 point)
    {
        return Vector2.Transform(point, LocalToWorldMatrix());
    }

    public Transform Rotated(float angle)
    {
        return this with { Rotation = Rotation + angle };
    }

    public Transform Translated(DoubleVector translation)
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
        result.Position = DoubleVector.Lerp(a.Position, b.Position, t);
        result.Rotation = Angle.Lerp(a.Rotation, b.Rotation, t);
        result.Scale = Vector2.Lerp(a.Scale, b.Scale, t);
        return result;
    }

    public Transform WithRotation(float rotation)
    {
        return this with { Rotation = rotation };
    }

    public Transform WithPosition(DoubleVector position)
    {
        return this with { Position = position };
    }
}
