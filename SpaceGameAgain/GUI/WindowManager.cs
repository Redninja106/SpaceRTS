using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
internal class WindowManager
{
    public float Scale { get; set; } = 2f;
    public Vector2 MousePosition { get; set; }

    private List<ElementWindow> windows = [];

    public bool IsAnyWindowHovered = false;

    public void RegisterWindow(ElementWindow window)
    {
        this.windows.Add(window);
    }

    public void Render(ICanvas canvas, float displayWidth, float displayHeight)
    {
        canvas.ResetState();
        canvas.Font(Program.font);
        canvas.Scale(Scale);

        foreach (var window in windows)
        {
            if (window.Visible)
            {
                canvas.PushState();
                window.Render(canvas, displayWidth, displayHeight);
                canvas.PopState();
            }
        }
    }

    public void Update(float displayWidth, float displayHeight)
    {
        MousePosition = Mouse.Position / Scale;

        IsAnyWindowHovered = false;
        foreach (var window in windows)
        {
            if (window.Visible)
            {
                if (window.Bounds.ContainsPoint(MousePosition))
                {
                    IsAnyWindowHovered = true;
                }

                window.Update(displayWidth, displayHeight);
            }
        }
    }
}

