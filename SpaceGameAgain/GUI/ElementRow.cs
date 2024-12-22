using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
internal class ElementRow(Element[] elements) : Element
{
    private readonly Vector2[] localPositions = elements.Select(e => Vector2.Zero).ToArray();

    public bool DrawBorder { get; set; } = false;

    public override void Render(ICanvas canvas)
    {
        if (DrawBorder)
        {
            canvas.Stroke(ShadowColor);
            canvas.DrawRect(2, 2, Width, Height);
            canvas.Stroke(ForegroundColor);
            canvas.DrawRect(0, 0, Width, Height);
        }

        for (int i = 0; i < elements.Length; i++)
        {
            canvas.PushState();
            canvas.Translate(localPositions[i]);
            elements[i].Render(canvas);
            canvas.PopState();
        }
    }

    public override void Update(float locationX, float locationY)
    {
        for (int i = 0; i < elements.Length; i++)
        {
            var pos = localPositions[i];
            elements[i].Update(locationX + pos.X, locationY + pos.Y);
        }
    }

    public override void UpdateSize(float containerWidth, float containerHeight)
    {
        Width = Height = 0;
        float left = Margin, right = containerWidth - Margin;
        for (int i = 0; i < elements.Length; i++)
        {
            var element = elements[i];
            element.UpdateSize(right - left - Margin, containerHeight);
            if (element.Alignment is not (Alignment.CenterRight or Alignment.TopRight or Alignment.BottomRight)) 
            {
                localPositions[i] = new(left, Margin);
                left += element.Width + element.Margin;
            }
            else
            {
                localPositions[i] = new(right - element.Width, Margin);
                right -= element.Width + element.Margin;
            }

            Height = MathF.Max(Height, element.Height);
        }
        Width = containerWidth;
        Height += 2 * Margin;
    }
}
