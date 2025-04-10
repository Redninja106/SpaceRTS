using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
internal abstract class GUIWindow
{
    public Vector2 Offset = new(0, 0);
    public Alignment Anchor = Alignment.TopLeft;
    public bool Visible = false;

    public Vector2 Cursor;
    public Rectangle LastItemBounds;
    public LayoutMode LayoutMode;
    public bool Hovered = false;

    public Rectangle CalculatedBounds => bounds;

    private Rectangle bounds;
    private List<DrawCommand> commands = [];

    private Vector2 mousePosition;

    public virtual void Update(GUIViewport viewport)
    {
        mousePosition = viewport.MousePosition;

        if (Visible)
        {
            bounds = new(viewport.Bounds.GetAlignedPoint(Anchor) + this.Offset, bounds.Size, this.Anchor);
            bounds.Size = Vector2.Zero;
            Cursor = this.bounds.Position;
            Layout();
            Hovered = bounds.ContainsPoint(viewport.MousePosition);
        }
        else
        {
            Hovered = false;
        }
    }

    public virtual void Layout()
    {
    }

    public void Text(string text, float size = 16)
    {
        LastItemBounds = Program.font.MeasureText(text, size);
        LastItemBounds.Position += Cursor + new Vector2(0, size);

        commands.Add(new DrawCommand.Text(text, size, Cursor + new Vector2(0, size)));

        UpdateLayout();
    }

    private void UpdateLayout()
    {
        if (LayoutMode == LayoutMode.Horizontal)
        {
            Cursor.X += LastItemBounds.Width;
        }
        else
        {
            Cursor.Y += LastItemBounds.Height;
        }

        bounds = bounds.Union(LastItemBounds);
    }

    public void Image(ITexture image)
    {
        LastItemBounds = new(Cursor.X, Cursor.Y, image.Width, image.Height);
        commands.Add(new DrawCommand.Image(image, LastItemBounds));
        UpdateLayout();
    }

    public bool LastItemHovered()
    {
        return Visible && LastItemBounds.ContainsPoint(mousePosition);
    }

    public bool LastItemClicked(MouseButton button)
    {
        return Visible && LastItemHovered() && Mouse.IsButtonPressed(button);
    }

    public void Render(ICanvas canvas, float displayWidth, float displayHeight)
    {
        canvas.PushState();

        canvas.Stroke(Color.Red);
        canvas.DrawRect(this.bounds);

        foreach (var command in commands)
        {
            command.Render(canvas);
        }

        commands.Clear();
        canvas.PopState();
    }
}

abstract class DrawCommand
{
    public abstract void Render(ICanvas canvas);

    public class Text(string text, float size, Vector2 position) : DrawCommand
    {
        public override void Render(ICanvas canvas)
        {
            canvas.Font(Program.font);
            canvas.DrawText(text, size, position);
        }
    }
    public class Image(ITexture image, Rectangle destination) : DrawCommand
    {
        public override void Render(ICanvas canvas)
        {
            canvas.DrawTexture(image, destination);
        }
    }
}
enum LayoutMode
{
    Vertical,
    Horizontal,
}
