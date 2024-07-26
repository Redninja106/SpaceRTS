using SimulationFramework;
using SimulationFramework.Drawing;
using SimulationFramework.Input;
using SpaceGame.Scenes;
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

        Vector2 delta = Vector2.Zero;

        Vector2 zoomTarget = this.ScreenToWorld(GameScene.ViewportMousePosition, false);

        float zoomFac = MathF.Pow(1.1f, zoom);
        VerticalSize = zoomFac;

        Vector2 newZoomTarget = this.ScreenToWorld(GameScene.ViewportMousePosition, false);
        
        this.Transform.Position += zoomTarget - newZoomTarget;

        if (Keyboard.IsKeyDown(Key.W))
            delta -= Vector2.UnitY;
        if (Keyboard.IsKeyDown(Key.A))
            delta -= Vector2.UnitX;
        if (Keyboard.IsKeyDown(Key.S))
            delta += Vector2.UnitY;
        if (Keyboard.IsKeyDown(Key.D))
            delta += Vector2.UnitX;

        Transform.Position += zoomFac * delta * Time.DeltaTime;
    }
}
