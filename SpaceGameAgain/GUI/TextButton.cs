﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
internal class TextButton : Element
{
    public string Text;
    public Action? click;
    public bool hovered;
    public bool pressed;
    public bool clicked;

    public bool FitContainer { get; set; } = false;

    public TextButton(string text, Action? click = null)
    {
        this.Text = text;
        this.click = click;
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
        canvas.Fill(ForegroundColor);
        DrawShadowedText(canvas, Text, TextSize, new(Margin));
        if (pressed)
        {
            canvas.PopState();
        }
    }

    public override void UpdateSize(float containerWidth, float containerHeight)
    {
        (Width, Height) = Program.font.MeasureText(Text, TextSize).Size + new Vector2(2 * Margin);

        if (FitContainer)
        {
            Width = containerWidth;
        }
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
