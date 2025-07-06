using SpaceGame.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
public class GUIViewport
{
    public Vector2 MousePosition;
    public float Scale => Program.UserOptions.GUIScale;
    public bool IsAnyWindowHovered;

    public float EffectiveWidth;
    public float EffectiveHeight;

    public Rectangle Bounds => new(0, 0, EffectiveWidth, EffectiveHeight);

    public List<GUIWindow> windows = [];
    public GUIWindow tooltipWindow = new(null)
    {
        Alignment = Alignment.BottomRight,
        Anchor = Alignment.TopLeft,
    };

    public GUIWindow popupWindow = new(null);

    public GUIViewport()
    {
        popupWindow.Viewport = tooltipWindow.Viewport = this;
    }

    public void Register(GUIWindow window)
    {
        window.Viewport = this;
        windows.Add(window);
    }

    public void UpdateWindowOcculusion(float displayWidth, float displayHeight)
    {
        EffectiveWidth = displayWidth / Scale;
        EffectiveHeight = displayHeight / Scale;

        MousePosition = Mouse.Position / Scale;

        IsAnyWindowHovered = false;
        foreach (var w in windows)
        {
            IsAnyWindowHovered |= w.Hovered;
        }

        // update popup window
        IsAnyWindowHovered |= popupWindow.Hovered;
    }

    public void SetTooltip(GUILayout layout)
    {
        tooltipWindow.SetLayout(layout);
    }

    public void OpenPopup(GUILayout layout, Vector2 viewportPosition, Alignment alignment)
    {
        popupWindow.SetLayout(layout);
        popupWindow.Offset = viewportPosition - Bounds.GetAlignedPoint(alignment);
        popupWindow.Anchor = alignment;
        popupWindow.Alignment = alignment;
    }

    public void ClosePopup()
    {
        popupWindow.SetLayout(null);
    }

    public void Render(ICanvas canvas)
    {
        canvas.Scale(Scale);

        foreach (var w in windows)
        {
            w.Render(canvas, canvas.Width / Scale, canvas.Height / Scale);
        }

        popupWindow.Render(canvas, canvas.Width / Scale, canvas.Height / Scale);
        tooltipWindow.Render(canvas, canvas.Width / Scale, canvas.Height / Scale);
    }

    public void Update()
    {
        foreach (var w in windows)
        {
            w.Update();
        }

        if (popupWindow.Layout != null)
        {
            if (!popupWindow.HasNewLayout && !popupWindow.Hovered)
            {
                if (Mouse.IsButtonPressed(MouseButton.Left) ||
                    Mouse.IsButtonPressed(MouseButton.Right) ||
                    Mouse.IsButtonPressed(MouseButton.Middle) ||
                    Keyboard.IsKeyPressed(Key.Escape))
                {
                    popupWindow.SetLayout(null);
                }
            }
        }

        popupWindow.Update();

        if (tooltipWindow.Layout != null)
        {
            tooltipWindow.Offset = MousePosition; 
            if (tooltipWindow.GetPosition().X - tooltipWindow.GetPredictedBounds().Width < 0)
            {
                tooltipWindow.Alignment = Alignment.BottomLeft;
            }
            else
            {
                tooltipWindow.Alignment = Alignment.BottomRight;
            }
            tooltipWindow.Update();
            tooltipWindow.SetLayout(null);
        }
    }
}

//interface IGUIWindowContainer
//{
//    void Render(ICanvas canvas, float displayWidth, float displayHeight);
//    void Update(GUIViewport viewport);
//}