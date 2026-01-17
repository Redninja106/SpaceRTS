using SimulationFramework;
using SimulationFramework.Drawing;
using SimulationFramework.Input;
using SpaceGame.Planets;
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

    public void ZoomTowards(float amount, Vector2 screenSpaceTarget)
    {
        DoubleVector zoomTarget = DoubleVector.FromVector2(this.ScreenToWorld(screenSpaceTarget, false));

        zoom += amount;

        float viewSize = 2 * float.Min(this.DisplayWidth, this.DisplayHeight);
        float minZoom = float.Log(viewSize / (128 * float.Sqrt(3)), 1.1f);
        float maxZoom = float.Log(50000, 1.1f);

        this.zoom = float.Clamp(zoom, minZoom, maxZoom);

        float zoomFac = float.Pow(1.1f, zoom);
        this.VerticalSize = zoomFac;

        DoubleVector newZoomTarget = DoubleVector.FromVector2(this.ScreenToWorld(screenSpaceTarget, false));
        
        this.Transform.Position -= newZoomTarget - zoomTarget;
    }

    public override void Update(int width, int height, float tickProgress)
    {
        base.Update(width, height, tickProgress);

        float z = 0;
        if (!Program.World.GUIViewport.IsAnyWindowHovered)
        {
            z -= Program.UserOptions.ScrollSpeed * Mouse.ScrollWheelDelta;

            if (Keyboard.IsKeyDown(Key.Plus))
            {
                z -= Time.DeltaTime * 20;
            }
            if (Keyboard.IsKeyDown(Key.Minus))
            {
                z += Time.DeltaTime * 20;
            }
        }

        if (Mouse.ScrollWheelDelta != 0)
        {
            ZoomTowards(z, Program.ViewportMousePosition);
        }

        DoubleVector delta = DoubleVector.Zero;
        if (Keyboard.IsKeyDown(Key.W))
        {
            delta -= DoubleVector.FromVector2(0, 1);
        }
        if (Keyboard.IsKeyDown(Key.A))
        {
            delta -= DoubleVector.FromVector2(1, 0);
        }
        if (Keyboard.IsKeyDown(Key.S))
        {
            delta += DoubleVector.FromVector2(0, 1);
        }
        if (Keyboard.IsKeyDown(Key.D))
        {
            delta += DoubleVector.FromVector2(1, 0);
        }

        Transform.Position += float.Pow(1.1f, zoom) * delta * Time.DeltaTime;
        
        if (DoubleVector.Distance(this.Transform.Position, DoubleVector.Zero) > 25000)
        {
            //this.Transform.Position = this.Transform.Position.Normalized() * 25000;
        }
    }
}
