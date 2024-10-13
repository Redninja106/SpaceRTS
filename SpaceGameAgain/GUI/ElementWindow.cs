using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
internal class ElementWindow
{
    public ElementStack Stack { get; } = new();

    public Alignment? Anchor { get; set; }

    public Vector2 Offset { get; set; }

    public float Width { get; set; }
    public float Height { get; set; }

    public void Update(int displayWidth, int displayHeight)
    {
        CalculateBounds(displayWidth, displayHeight, out var bounds, out var content);
        Stack.UpdateSize(content.Width, content.Height);

        Stack.Update(content.Position.X, content.Position.Y);
    }

    public void Render(ICanvas canvas, int displayWidth, int displayHeight)
    {
        CalculateBounds(displayWidth, displayHeight, out var bounds, out var content);
        
        canvas.Fill(Color.FromHSV(0, 0, .15f));
        canvas.DrawRect(bounds);
        canvas.Stroke(Color.FromHSV(0, 0, .25f));
        bounds.Position -= new Vector2(.5f, .5f);
        bounds.Size += new Vector2(1, 1);
        canvas.DrawRect(bounds);

        canvas.Translate(content.Position);
        Stack.Render(canvas);
    }

    public void CalculateBounds(int displayWidth, int displayHeight, out Rectangle bounds, out Rectangle content)
    {
        Rectangle viewport = new(0, 0, displayWidth, displayHeight);

        Vector2 origin = Offset;
        if (Anchor is not null)
        {
            origin = Offset + viewport.GetAlignedPoint(Anchor!.Value);
        }

        bounds = new Rectangle(origin.X, origin.Y, Width, Height, Anchor ?? Alignment.TopLeft);

        content = bounds;
        content.Position += new Vector2(Element.Border);
        content.Size -= new Vector2(2 * Element.Border);
    }
}
