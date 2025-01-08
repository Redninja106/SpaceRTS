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
    public float zoom;

    public override void Update(int width, int height)
    {
        base.Update(width, height);

        zoom -= Mouse.ScrollWheelDelta;

        FixedVector2 delta = FixedVector2.Zero;

        FixedVector2 zoomTarget = FixedVector2.FromVector2(this.ScreenToWorld(Program.ViewportMousePosition, false));

        float zoomFac = MathF.Pow(1.1f, zoom);
        VerticalSize = zoomFac;

        FixedVector2 newZoomTarget = FixedVector2.FromVector2(this.ScreenToWorld(Program.ViewportMousePosition, false));
        
        this.Transform.Position += zoomTarget - newZoomTarget;

        if (Keyboard.IsKeyDown(Key.W))
            delta -= FixedVector2.FromVector2(0, 1);
        if (Keyboard.IsKeyDown(Key.A))
            delta -= FixedVector2.FromVector2(1, 0);
        if (Keyboard.IsKeyDown(Key.S))
            delta += FixedVector2.FromVector2(0, 1);
        if (Keyboard.IsKeyDown(Key.D))
            delta += FixedVector2.FromVector2(1, 0);

        Transform.Position += zoomFac * delta * Time.DeltaTime;
    }
}
