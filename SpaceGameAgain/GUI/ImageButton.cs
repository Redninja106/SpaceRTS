using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
internal class ImageButton : Element
{
    public Action? click;
    public bool hovered;
    public bool pressed;
    public bool clicked;
    public ITexture texture;
    public float width, height;

    public bool FitContainer { get; set; } = false;

    public ImageButton(ITexture texture, float width, float height, Action? click = null)
    {
        this.texture = texture;
        this.click = click;
        this.width = width;
        this.height = height;
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Fill(ShadowColor);
        canvas.DrawRect(1, 2, 1 + Width, Height);
        canvas.Fill(hovered ? Color.Red : Color.FromHSV(0, 0, .4f));
        if (pressed)
        {
            canvas.Translate(1, 2);
            canvas.PushState();
        }
        canvas.DrawRect(0, 0, Width, Height);
        canvas.DrawTexture(texture, 0, 0, Width, Height);
        if (pressed)
        {
            canvas.PopState();
        }
    }

    public override void UpdateSize(float containerWidth, float containerHeight)
    {
        (Width, Height) = new Vector2(width + 2 * Margin, height + 2 * Margin);
    }

    public override void Update(float locationX, float locationY)
    {
        clicked = false;

        Rectangle bounds = new(locationX, locationY, Width, Height);
        hovered = bounds.ContainsPoint(Program.ViewportMousePosition);

        if (hovered && Mouse.IsButtonPressed(MouseButton.Left))
        {
            pressed = true;
        }

        if (!hovered)
        {
            pressed = false;
        }

        if (pressed && Mouse.IsButtonReleased(MouseButton.Left))
        {
            pressed = false;
            if (hovered)
            {
                clicked = true;
                click?.Invoke();
            }
        }

        base.Update(locationX, locationY);
    }
}
