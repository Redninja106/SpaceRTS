using ImGuiNET;
using SimulationFramework;
using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SpaceGame;
public abstract class Camera : IInspectable
{
    public Transform SmoothTransform = Transform.Default;
    public Transform Transform = Transform.Default;

    public int DisplayWidth { get; private set; }
    public int DisplayHeight { get; private set; }

    public float VerticalSize { get; set; } = 10;
    public float SmoothVerticalSize = 10;

    public float InterpolationFactor { get; set; } = 0.000001f;

    public float AspectRatio => DisplayWidth / DisplayHeight;

    public virtual void Update(int width, int height, float tickProgress)
    {
        DisplayWidth = width;
        DisplayHeight = height;

        Transform target = Transform;

        SmoothTransform.Position = DoubleVector.Lerp(SmoothTransform.Position, target.Position, 1f - MathF.Pow(InterpolationFactor, Time.DeltaTime));
        SmoothVerticalSize = float.Lerp(SmoothVerticalSize, VerticalSize, 1f - MathF.Pow(InterpolationFactor, Time.DeltaTime));
    }

    public void RenderSetup(ICanvas canvas)
    {
        // canvas.Transform(ScreenFromWorldMatrix());
        canvas.Clear(Color.Black);
    }

    public Matrix3x2 ScreenFromWorldMatrix(bool interpolated = true)
    {
        return (interpolated ? SmoothTransform : Transform).LocalFromWorldMatrix() * ScreenFromLocalMatrix(interpolated);
    }

    public Matrix3x2 WorldFromScreenMatrix(bool interpolated = true)
    {
        return LocalFromScreenMatrix(interpolated) * (interpolated ? SmoothTransform : Transform).WorldFromLocalMatrix();
    }

    public Matrix3x2 ScreenFromLocalMatrix(bool interpolated = true)
    {
        Matrix3x2 result = Matrix3x2.Identity;
        result = Matrix3x2.CreateTranslation(DisplayWidth / 2f, DisplayHeight / 2f) * result;
        result = Matrix3x2.CreateScale(DisplayHeight / (interpolated ? SmoothVerticalSize : VerticalSize)) * result;
        return result;
    }

    public Matrix3x2 LocalFromScreenMatrix(bool interpolated = true)
    {
        Matrix3x2 result = Matrix3x2.Identity;
        result = Matrix3x2.CreateScale((interpolated ? SmoothVerticalSize : VerticalSize) / DisplayHeight) * result;
        result = Matrix3x2.CreateTranslation(-DisplayWidth / 2f, -DisplayHeight / 2f) * result;
        return result;
    }

    public Vector2 ScreenToWorld(Vector2 point, bool interpolated = true)
    {
        return Vector2.Transform(point, WorldFromScreenMatrix(interpolated));
    }

    public Vector2 ScreenToLocal(Vector2 point, bool interpolated = true)
    {
        return Vector2.Transform(point, LocalFromScreenMatrix(interpolated));
    }

    public Vector2 WorldToScreen(Vector2 point, bool interpolated = true)
    {
        return Vector2.Transform(point, ScreenFromWorldMatrix(interpolated));
    }

    public Vector2 LocalToScreen(Vector2 point, bool interpolated = true)
    {
        return Vector2.Transform(point, ScreenFromLocalMatrix(interpolated));
    }

    public float ScreenDistanceToWorldDistance(float distance)
    {
        return Vector2.Distance(ScreenToWorld(Vector2.Zero), ScreenToWorld(new(distance, 0)));
    }

    public Matrix3x2 CreateRelativeMatrix(Transform transform)
    {
        DoubleVector positionDiff = transform.Position - this.SmoothTransform.Position;
        float rotationDiff = transform.Rotation - this.SmoothTransform.Rotation;
        Vector2 scaleDiff = transform.Scale / this.SmoothTransform.Scale;

        // TODO: calculate matrix more precise to actually take advantage of double precision!

        double displayScale = (double)DisplayHeight / (double)SmoothVerticalSize;

        //Matrix3x2 scaleRotation = 
        //    Matrix3x2.CreateScale(scaleDiff) *
        //    Matrix3x2.CreateRotation(rotationDiff) *
        //    Matrix3x2.CreateScale(DisplayHeight / SmoothVerticalSize);

        //DoubleVector translation = positionDiff + displayScale * new DoubleVector(DisplayWidth / 2f, DisplayHeight / 2f);

        //scaleRotation.Translation = translation.ToVector2();
        //return scaleRotation;

        Matrix3x2 result =
            Matrix3x2.CreateScale(scaleDiff) *
            Matrix3x2.CreateRotation(rotationDiff) *
            Matrix3x2.CreateScale((float)displayScale) *
            Matrix3x2.CreateTranslation((displayScale * positionDiff).ToVector2()) *
            Matrix3x2.CreateTranslation(DisplayWidth / 2f, DisplayHeight / 2f);

        return result;
    }

    public virtual void Layout()
    {
        Transform.Layout();
        ImGui.Text($"display size: {DisplayWidth}x{DisplayHeight}");
        float vs = VerticalSize;
        ImGui.DragFloat("vertical size", ref vs);
        VerticalSize = vs;

    }
}