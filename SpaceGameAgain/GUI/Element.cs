using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
internal abstract class Element
{
    public const float DefaultMargin = 6;
    public const float Border = 6;
    public const float TextSize = 16;
    public static readonly Color ForegroundColor = Color.FromHSV(0, 0, .85f);
    public static readonly Color ShadowColor = Color.FromHSV(0, 0, .05f);
    public static readonly Color BackgroundColor = Color.FromHSV(0, 0, .15f);

    public float Width { get; set; }
    public float Height { get; set; }
    public float Margin { get; set; } = DefaultMargin;
    public Alignment Alignment { get; set; }
    public abstract void UpdateSize(float containerWidth, float containerHeight);
    public virtual void Update(float locationX, float locationY)
    {
    }
    public abstract void Render(ICanvas canvas);

    public static Vector2 MeasureText(string text, IFont font, float size)
    {
        var canvas = Graphics.GetOutputCanvas();

        canvas.PushState();
        canvas.Font(font);
        canvas.FontSize(size);
        var result = canvas.MeasureText(text, TextBounds.Largest);
        canvas.PopState();
        return result;
    }
    public static void DrawShadowedText(ICanvas canvas, string text, float size, Vector2 position, Alignment alignment = Alignment.TopLeft)
    {
        const float offset = 1f/16f;

        canvas.FontSize(size);
        canvas.Translate(new(size * offset));
        canvas.Fill(ShadowColor);
        canvas.DrawText(text, position, alignment, origin: TextBounds.Largest);
        canvas.Translate(new(-size * offset));
        canvas.Fill(ForegroundColor);
        canvas.DrawText(text, position, alignment, origin: TextBounds.Largest);
    }
}
