using SimulationFramework;
using SimulationFramework.Drawing;
using SimulationFramework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class FreeCamera : Camera
{
    public float zoom = 10;

    public FreeCamera()
    {
        SmoothVerticalSize = VerticalSize = MathF.Pow(1.1f, zoom);
    }

    public override void Update(int width, int height, float tickProgress)
    {
        base.Update(width, height, tickProgress);

        zoom -= Mouse.ScrollWheelDelta;
        if (Keyboard.IsKeyDown(Key.Plus))
            zoom -= Time.DeltaTime;
        if (Keyboard.IsKeyDown(Key.Minus))
            zoom += Time.DeltaTime;

        DoubleVector delta = DoubleVector.Zero;
        DoubleVector zoomTarget = DoubleVector.FromVector2(this.ScreenToWorld(Program.ViewportMousePosition, false));

        float zoomFac = MathF.Pow(1.1f, zoom);
        VerticalSize = zoomFac;
            
        DoubleVector newZoomTarget = DoubleVector.FromVector2(this.ScreenToWorld(Program.ViewportMousePosition, false));
        this.Transform.Position -= newZoomTarget - zoomTarget;

        if (Keyboard.IsKeyDown(Key.W))
            delta -= DoubleVector.FromVector2(0, 1);
        if (Keyboard.IsKeyDown(Key.A))
            delta -= DoubleVector.FromVector2(1, 0);
        if (Keyboard.IsKeyDown(Key.S))
            delta += DoubleVector.FromVector2(0, 1);
        if (Keyboard.IsKeyDown(Key.D))
            delta += DoubleVector.FromVector2(1, 0);

        Transform.Position += zoomFac * delta * Time.DeltaTime;
    }
}
