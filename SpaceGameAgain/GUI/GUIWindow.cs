using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
internal class GUIWindow
{
    public static Color DefaultTextColor = Color.FromHSV(0, 0, .65f);

    public Vector2 Offset = new(0, 0);
    public Alignment Anchor = Alignment.TopLeft;
    public bool Visible = false;
    public bool Hovered = false;

    public Vector2 Cursor;
    public Rectangle LastItemBounds;
    public LayoutMode LayoutMode;

    private Stack<LayoutState> stateStack = [];

    public Rectangle CalculatedBounds => bounds;

    private Rectangle lastFrameBounds;
    private Rectangle bounds;
    private List<DrawCommand> commands = [];

    private Vector2 mousePosition;

    public float Margin = 3;

    public virtual void Update(GUIViewport viewport)
    {
        mousePosition = viewport.MousePosition;

        if (Visible)
        {
            Hovered = bounds.ContainsPoint(viewport.MousePosition);

            lastFrameBounds = bounds;
            bounds = new(viewport.Bounds.GetAlignedPoint(Anchor) + this.Offset, bounds.Size, this.Anchor);
            bounds.Size = Vector2.Zero;
            LastItemBounds = new(bounds.X, bounds.Y, 0, 0);
            Cursor = this.bounds.Position;
        }
        else
        {
            Hovered = false;
            if (commands.Count > 0)
            {
                commands.Clear();
            }
        }
    }

    public virtual void Layout()
    {
    }

    public void Text(string text, float size = 16, Color? color = null, TextStyle style = TextStyle.Regular)
    {
        UpdateLayout();
        
        LastItemBounds = Program.font.MeasureText(text, size);
        LastItemBounds.Position += Cursor + new Vector2(0, size);
        LastItemBounds.X -= Margin;
        LastItemBounds.Y -= Margin;
        LastItemBounds.Width += Margin * 2;
        LastItemBounds.Height += Margin * 2;

        commands.Add(new DrawCommand.Text(text, size, Cursor + new Vector2(0, size), color, style));
    }

    public bool TextButton(string text, float size = 16, bool disabled = false)
    {
        UpdateLayout();
        
        LastItemBounds.Width = Program.font.MeasureText(text, size).Width;
        LastItemBounds.Height = size;
        LastItemBounds.Position = Cursor + new Vector2(0, Margin);
        LastItemBounds.X -= Margin;
        LastItemBounds.Y -= Margin;
        LastItemBounds.Width += Margin * 2;
        LastItemBounds.Height += Margin * 2;

        if (LastItemHovered())
        {
            commands.Add(new DrawCommand.Rectangle(LastItemBounds, Color.Gray, true));
        }
        else
        {
            commands.Add(new DrawCommand.Rectangle(LastItemBounds, Color.FromHSV(.6f, .25f, .25f), true));
        }

        commands.Add(new DrawCommand.Rectangle(LastItemBounds with { X = LastItemBounds.X + 1, Y = LastItemBounds.Y + 1 }, new Color(28, 33, 38), false));
        commands.Add(new DrawCommand.Rectangle(LastItemBounds, new Color(70, 79, 89), false));
        
        commands.Add(new DrawCommand.Text(text, size, Cursor + new Vector2(0, size)));

        return LastItemClicked(MouseButton.Left);
    }

    protected void UpdateLayout()
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
        Image(image, new(image.Width, image.Height));
    }

    public void Image(ITexture image, Vector2 size)
    {
        UpdateLayout();
        LastItemBounds = new(Cursor.X, Cursor.Y, size.X, size.Y);
        commands.Add(new DrawCommand.Image(image, LastItemBounds));
    }

    public bool LastItemHovered()
    {
        return Visible && LastItemBounds.ContainsPoint(mousePosition);
    }

    public bool LastItemClicked(MouseButton button)
    {
        return Visible && LastItemHovered() && Mouse.IsButtonPressed(button);
    }

    public virtual void Render(ICanvas canvas, float displayWidth, float displayHeight)
    {
        UpdateLayout();

        canvas.PushState();

        canvas.Fill(new Color(12, 17, 23));
        canvas.DrawRect(this.bounds);

        canvas.Stroke(new Color(28, 33, 38));
        canvas.DrawRect(this.bounds with { X = bounds.X + 1, Y = bounds.Y + 1 });

        canvas.Stroke(new Color(70, 79, 89));
        canvas.DrawRect(this.bounds);

        foreach (var command in commands)
        {
            command.Render(canvas);
        }

        commands.Clear();
        canvas.PopState();
    }

    internal void ProgressBar(float progress, float width)
    {
        UpdateLayout();
        
        LastItemBounds = new(Cursor.X, Cursor.Y, width, 5f);

        commands.Add(new DrawCommand.Rectangle(LastItemBounds, Color.Gray, true));
        commands.Add(new DrawCommand.Rectangle(LastItemBounds with { Width = LastItemBounds.Width * progress }, Color.DarkGray, true));
    }

    public void PushState()
    {
        stateStack.Push(new()
        {
            Cursor = this.Cursor,
            LastItemBounds = this.LastItemBounds,
            LayoutMode = this.LayoutMode,
        });
    }

    public void PopState()
    {
        UpdateLayout();
        var state = stateStack.Pop();

        Cursor = state.Cursor;
        LastItemBounds = state.LastItemBounds;
        LayoutMode = state.LayoutMode;
    }

    public void Separator()
    {
        UpdateLayout();

        LastItemBounds = new(Cursor.X, Cursor.Y + Margin, this.lastFrameBounds.Width - 2 * Margin, 1);
        Cursor.Y += Margin;

        commands.Add(new DrawCommand.Rectangle(LastItemBounds, DefaultTextColor, true));
    }

    private struct LayoutState
    {
        public Vector2 Cursor;
        public Rectangle LastItemBounds;
        public LayoutMode LayoutMode;
    }
}

abstract class DrawCommand
{
    public abstract void Render(ICanvas canvas);

    public class Text(string text, float size, Vector2 position, Color? color = null, TextStyle style = TextStyle.Regular) : DrawCommand
    {
        public override void Render(ICanvas canvas)
        {
            canvas.Fill(color ?? Color.FromHSV(0, 0, .65f));
            canvas.Font(Program.font);
            canvas.DrawText(text, size, position, style);
        }
    }
    public class Image(ITexture image, SimulationFramework.Rectangle destination) : DrawCommand
    {
        public override void Render(ICanvas canvas)
        {
            canvas.DrawTexture(image, destination);
        }
    }
    public class Rectangle(SimulationFramework.Rectangle rectangle, Color color, bool fill) : DrawCommand
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
            canvas.DrawRect(rectangle);
        }
    }
}
enum LayoutMode
{
    Vertical,
    Horizontal,
}
