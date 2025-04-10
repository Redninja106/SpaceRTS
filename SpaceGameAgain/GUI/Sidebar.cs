using SpaceGame.GUI;
using System.Numerics;

//class Sidebar
//{
//    public Alignment Anchor { get; }

//    public float MinWidth { get; set; }
//    public float MaxWidth { get; set; }
//    public float Width { get; set; }

//    public ElementStack Stack { get; set; } = new();

//    public Sidebar(Alignment anchor, float minWidth, float maxWidth)
//    {
//        Anchor = anchor;
//        MinWidth = minWidth;
//        MaxWidth = maxWidth;
//    }

//    public void Update(int displayWidth, int displayHeight)
//    {
//        Rectangle vp = new(0, 0, displayWidth, displayHeight);
//        Vector2 origin = vp.GetAlignedPoint(Anchor);
//        Rectangle bounds = new(origin.X, origin.Y, Width, displayHeight, Anchor);
//        Rectangle contentRegion = bounds;
//        contentRegion.Position += new Vector2(Element.Border);
//        contentRegion.Size -= new Vector2(2 * Element.Border);

//        Stack.UpdateSize(contentRegion.Width, contentRegion.Height);

//        Stack.Update(contentRegion.Position.X, contentRegion.Position.Y);
//    }

//    public void Render(ICanvas canvas)
//    {
//        canvas.Fill(Color.FromHSV(0, 0, .15f));

//        Rectangle vp = new(0, 0, canvas.Width, canvas.Height);
//        Vector2 origin = vp.GetAlignedPoint(Anchor);
//        var bounds = new Rectangle(origin.X, origin.Y, Width, canvas.Height, Anchor);
//        canvas.DrawRect(bounds);

//        Rectangle contentRegion = bounds;
//        contentRegion.Position += new Vector2(Element.Border);
//        contentRegion.Size -= new Vector2(2 * Element.Border);

//        canvas.Translate(contentRegion.Position);
//        Stack.Render(canvas);
//    }
//}