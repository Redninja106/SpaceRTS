using Newtonsoft.Json.Linq;
using SpaceGame.GUI;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal static class Minimap
{
    private static ITexture? minimapTexture;
    public const int TextureSize = 250;
    public const float WorldSize = 2000;
    public const float DisplayScale = 1;

    public static void Layout(GUIWindow window)
    {
        window.Anchor = window.Alignment = Alignment.BottomRight;

        if (minimapTexture != null)
        {
            window.Image(minimapTexture, new Vector2(minimapTexture.Width * DisplayScale, minimapTexture.Height * DisplayScale));
            if (window.LastItemHeld(MouseButton.Middle))
            {
                Vector2 mousePosition = window.GetLastItemMousePosition();
                Vector2 worldPosition = (mousePosition / TextureSize - new Vector2(.5f)) * WorldSize;
                Program.World.Camera.Transform.Position = DoubleVector.FromVector2(worldPosition);
            }
        }
    }

    public static void Render()
    {
        minimapTexture ??= Graphics.CreateTexture(TextureSize, TextureSize);
        var canvas = minimapTexture.GetCanvas();
        canvas.ResetState();
        canvas.Clear(ColorF.Black);

        foreach (var planet in Program.World.Planets)
        {
            if (planet.orbit != null)
            {
                canvas.Stroke(Color.White with { A = 50 });
                canvas.DrawCircle(WorldPointToMinimapPoint(planet.orbit.center.Transform.Position.ToVector2()), WorldSizeToMinimapSize(planet.orbit.radius));
            }
            canvas.Fill(Color.Gray);
            canvas.DrawCircle(WorldPointToMinimapPoint(planet.Transform.Position.ToVector2()), float.Max(1f, (planet.Radius / WorldSize) * TextureSize));
        }

        foreach (var ship in Program.World.Ships)
        {
            RenderUnit(canvas, ship);
        }
        foreach (var ship in Program.World.Structures)
        {
            RenderUnit(canvas, ship);
        }
        foreach (var ship in Program.World.Stations)
        {
            RenderUnit(canvas, ship);
        }

        var camera = Program.World.Camera;
        Vector2 cameraPos = WorldPointToMinimapPoint(camera.SmoothTransform.Position.ToVector2());
        float cameraHeight = WorldSizeToMinimapSize(2 * camera.SmoothVerticalSize);
        float cameraWidth = WorldSizeToMinimapSize(2 * camera.SmoothVerticalSize / camera.AspectRatio);
        Rectangle cameraRect = new(cameraPos.X, cameraPos.Y, cameraHeight, cameraWidth, Alignment.Center);

        canvas.Stroke(Color.White);
        canvas.DrawRect(cameraRect);
    }

    private static void RenderUnit(ICanvas canvas, Unit unit)
    {
        if (Program.World.Collision.IsClientVisible(unit.Transform.Position))
        {
            canvas.Fill(unit.Team.GetRelationColor(Program.World.PlayerTeam));
            canvas.DrawCircle(WorldPointToMinimapPoint(unit.Transform.Position.ToVector2()), float.Max(1f, ((float)unit.GetCollisionRadius() / WorldSize) * TextureSize));
        }
    }

    private static Vector2 WorldPointToMinimapPoint(Vector2 point)
    {
        float x = TextureSize * (point.X / WorldSize + .5f);
        float y = TextureSize * (point.Y / WorldSize + .5f);
        return new(x, y);
    }

    private static float WorldSizeToMinimapSize(float value)
    {
        return TextureSize * ((float)value / WorldSize);
    }
}
