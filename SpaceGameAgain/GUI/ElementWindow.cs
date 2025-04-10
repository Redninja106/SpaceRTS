using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
//internal class ElementWindow
//{
//    public Element? RootElement { get; set; }

//    public Alignment Anchor { get; set; } = Alignment.TopLeft;
//    public Alignment Origin { get; set; } = Alignment.TopLeft;

//    public Vector2 Offset { get; set; }

//    public float Width { get; set; } = 100;
//    public float Height { get; set; } = 100;
//    public bool Visible { get; set; } = false;

//    public float Border = Element.DefaultMargin;

//    public Rectangle Bounds { get; set; }

//    public virtual void Update(float displayWidth, float displayHeight)
//    {
//        CalculateBounds(displayWidth, displayHeight, out var bounds, out var content);
//        RootElement?.UpdateSize(content.Width, content.Height);
//        RootElement?.Update(content.Position.X, content.Position.Y);
//    }

//    public virtual void Render(ICanvas canvas, float displayWidth, float displayHeight)
//    {
//        CalculateBounds(displayWidth, displayHeight, out var bounds, out var content);

//        canvas.Fill(Color.FromHSV(0, 0, .15f));
//        canvas.DrawRect(bounds);
//        canvas.Stroke(Color.FromHSV(0, 0, .25f));
//        bounds.Position -= new Vector2(.5f, .5f);
//        bounds.Size += new Vector2(1, 1);
//        canvas.DrawRect(bounds);
        
//        canvas.Translate(content.Position);
//        RootElement?.Render(canvas);
//    }

//    public virtual void CalculateBounds(float displayWidth, float displayHeight, out Rectangle bounds, out Rectangle content)
//    {
//        Rectangle viewport = new(0, 0, displayWidth, displayHeight);

//        Vector2 origin = Offset + viewport.GetAlignedPoint(Anchor);

//        bounds = new Rectangle(origin.X, origin.Y, Width, Height, Origin);

//        content = bounds;
//        content.Position += new Vector2(Border);
//        content.Size -= new Vector2(2 * Border);

//        Bounds = bounds;
//    }

//    public void Hide()
//    {
//        Visible = false;
//    }

//    public void Show()
//    {
//        Visible = true;
//    }
//}
