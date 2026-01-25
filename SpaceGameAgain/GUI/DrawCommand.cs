using SpaceGame.Rendering;

namespace SpaceGame.GUI;

public abstract class DrawCommand
{
    public abstract void Render(ICanvas canvas);

    public class Text(string text, float size, Vector2 position, Color? color = null, TextStyle style = TextStyle.Regular) : DrawCommand
    {
        public override void Render(ICanvas canvas)
        {
            canvas.Font(Program.font);
            canvas.Fill(Color.FromHSV(0, 0, .2f) with { A = color == null ? (byte)255 : color.Value.A }); 
            canvas.DrawText(text, size, position + new Vector2(size/24f), style);
            canvas.Fill(color ?? Color.FromHSV(0, 0, .65f));
            canvas.DrawText(text, size, position, style);
        }
    }
    public class Image(ITexture image, SimulationFramework.Rectangle destination, ColorF tint) : DrawCommand
    {
        public override void Render(ICanvas canvas)
        {
            canvas.DrawTexture(image, destination, tint);
        }
    }

    public class Rectangle(SimulationFramework.Rectangle rectangle, Color color, bool fill, float radius = 0) : DrawCommand
    {
        public override void Render(ICanvas canvas)
        {
            if (fill)
            {
                canvas.Fill(color);
            }
            else
            {
                canvas.Stroke(color);
                canvas.StrokeWidth(1);
            }

            if (radius > 0)
            {
                canvas.DrawRoundedRect(rectangle, radius);
            }
            else
            {
                canvas.DrawRect(rectangle);
            }
        }
    }

    internal class Model(SpriteModel model, Vector2 position, Vector2 size) : DrawCommand
    {
        public override void Render(ICanvas canvas)
        {
            canvas.PushState();
            canvas.Translate(position);
            canvas.Scale(size.X / model.Width, size.Y / model.Height);
            model.Render(canvas, Transform.Default, ColorF.White);
            canvas.PopState();
        }
    }
}
