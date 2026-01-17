using NAudio.Wave;
using Newtonsoft.Json.Linq;
using SpaceGame.GUI;
using SpaceGame.Planets;
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
    public const int BaseTextureSize = 250;
    public static float TextureSize => BaseTextureSize * Program.UserOptions.GUIScale;
    public const float DisplayScale = 1;
    private static SphereOfInfluence focus;

    public static void Layout(GUIWindow window)
    {
        window.Margin = 2;
        window.Anchor = window.Alignment = Alignment.BottomRight;

        if (minimapTexture != null)
        {
            window.Image(minimapTexture, new Vector2(minimapTexture.Width / Program.UserOptions.GUIScale, minimapTexture.Height / Program.UserOptions.GUIScale));
            if (window.LastItemHeld(MouseButton.Middle) || window.LastItemHeld(MouseButton.Left))
            {
                Vector2 mousePosition = window.GetLastItemMousePosition() - new Vector2(window.Margin);
                Vector2 worldPosition = ((mousePosition / BaseTextureSize - new Vector2(.5f)) * focus.Radius * 2) + focus.planet.Transform.Position.ToVector2();
                Program.World.Camera.Transform.Position = DoubleVector.FromVector2(worldPosition);
            }

            if (window.LastItemHovered())
            {
                if (Program.CurrentScene.Camera is FreeCamera freeCam)
                {
                    Vector2 mousePosition = window.GetLastItemMousePosition() - new Vector2(window.Margin);
                    Vector2 worldPosition = ((mousePosition / BaseTextureSize - new Vector2(.5f)) * focus.Radius * 2) + focus.planet.Transform.Position.ToVector2();
                    Vector2 screenPosition = freeCam.WorldToScreen(worldPosition);
                    freeCam.ZoomTowards(-Mouse.ScrollWheelDelta, screenPosition);
                }
            }
        }
    }

    public static void Render()
    {
        focus = Program.World.Camera.FocusedSphereOfInfluence ?? Program.World.Planets[0].SphereOfInfluence;
        if (focus.planet.orbit != null)
        {
            //focus = ((Planet)focus.planet.orbit.center).SphereOfInfluence;
        }

        if ((int)(BaseTextureSize * Program.UserOptions.GUIScale) != minimapTexture?.Width)
        {
            minimapTexture?.Dispose();
            minimapTexture = Graphics.CreateTexture((int)TextureSize, (int)TextureSize);
            minimapTexture.WrapModeX = WrapMode.Clamp;
            minimapTexture.WrapModeY = WrapMode.Clamp;
        }
        var canvas = minimapTexture.GetCanvas();
        canvas.ResetState();
        canvas.Clear(ColorF.Black);

        foreach (var planet in Program.World.Planets)
        {
            if (planet.orbit != null && planet.Prototype.Name != "star")
            {
                canvas.Stroke(Color.White with { A = 10 });
                canvas.DrawCircle(WorldPointToMinimapPoint(planet.orbit.center.Transform.Position.ToVector2()), WorldSizeToMinimapSize((float)planet.orbit.radius));
            }
            canvas.Fill(Color.Gray);
            canvas.DrawCircle(WorldPointToMinimapPoint(planet.Transform.Position.ToVector2()), float.Max(WorldSizeToMinimapSize(planet.Radius), 1f));
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
        Vector2 cameraPos = WorldPointToMinimapPoint(camera.SmoothTransform.Position).ToVector2();
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
            canvas.Stroke(unit.Team.GetRelationColor(Program.World.PlayerTeam));
            canvas.DrawCircle(WorldPointToMinimapPoint(unit.Transform.Position.ToVector2()), float.Clamp(((float)unit.GetCollisionRadius() / (2 * focus.Radius)) * TextureSize, 3f, 5f));
        }
    }
    private static DoubleVector WorldPointToMinimapPoint(DoubleVector point)
    {
        double x = TextureSize * ((point.X - focus.planet.Transform.Position.X) / (2 * focus.Radius) + .5);
        double y = TextureSize * ((point.Y - focus.planet.Transform.Position.Y) / (2 * focus.Radius) + .5);
        return new(x, y);
    }

    private static Vector2 WorldPointToMinimapPoint(Vector2 point)
    {
        float x = TextureSize * ((point.X - (float)focus.planet.Transform.Position.X) / (2 * focus.Radius) + .5f);
        float y = TextureSize * ((point.Y - (float)focus.planet.Transform.Position.Y) / (2 * focus.Radius) + .5f);
        return new(x, y);
    }

    private static float WorldSizeToMinimapSize(float value)
    {
        return TextureSize * ((float)value / (2 * focus.Radius));
    }
}
