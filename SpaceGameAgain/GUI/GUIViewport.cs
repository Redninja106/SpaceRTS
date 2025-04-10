using SpaceGame.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
internal class GUIViewport
{
    public Vector2 MousePosition;
    public float Scale = 1;
    public bool IsAnyWindowHovered;

    public float EffectiveWidth;
    public float EffectiveHeight;

    public Rectangle Bounds => new(0, 0, EffectiveWidth, EffectiveHeight);

    public List<GUIWindow> windows = [];

    public void Register(GUIWindow window)
    {
        windows.Add(window);
    }

    public void Update(float displayWidth, float displayHeight)
    {
        EffectiveWidth = displayWidth / Scale;
        EffectiveHeight = displayHeight / Scale;

        MousePosition = Mouse.Position / Scale;

        IsAnyWindowHovered = false;
        foreach (var w in windows)
        {
            w.Update(this);
            IsAnyWindowHovered |= w.Hovered;
        }
    }

    public void Render(ICanvas canvas)
    {
        canvas.Scale(Scale);

        foreach (var w in windows)
        {
            if (w.Visible)
            {
                w.Render(canvas, canvas.Width / Scale, canvas.Height / Scale);
            }
        }
    }
}
