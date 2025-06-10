using SpaceGame.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
internal sealed class GUIWindow
{
    public static Color DefaultTextColor = Color.FromHSV(0, 0, .65f);

    public Vector2 Offset = new(0, 0);
    /// <summary>
    /// The anchor point (on the viewport) of the window.
    /// </summary>
    public Alignment Anchor = Alignment.TopLeft;
    /// <summary>
    /// The alignment of the window to the anchor point.
    /// </summary>
    public Alignment Alignment = Alignment.TopLeft;
    public bool Visible = true;
    public bool Hovered = false;
    

    public Rectangle LastItemBounds;

    private Stack<LayoutScope> scopes = [];
    private LayoutScope currentScope;

    public Rectangle CalculatedBounds => predictedWindowBounds;


    private Rectangle predictedWindowBounds;
    private Rectangle currentWindowBounds;
    private List<DrawCommand> commands = [];

    private Vector2 mousePosition;

    public float Margin = 3;

    private GUILayout? layout;
    public bool HasNewLayout;

    public GUILayout? Layout => layout;

    [DebugOverlay]
    public static bool ShowGUIItemBounds;


    public GUIWindow(GUILayout? layout)
    {
        SetLayout(layout);
    }

    public void SetLayout(GUILayout? layout)
    {
        if (this.layout != layout) 
        {
            this.layout = layout;
            HasNewLayout = layout != null;
        }
    }

    public void Update(GUIViewport viewport)
    {
        commands.Clear();

        mousePosition = viewport.MousePosition;

        // finalWindowBounds = currentWindowBounds;

        if (Visible && layout != null)
        {
            // do an invisible layout to determine approximate window size
            Visible = false;
            DoLayout(viewport, Vector2.Zero);
            Visible = true;
            predictedWindowBounds = currentWindowBounds;
        }

        DoLayout(viewport, predictedWindowBounds.Size);
        HasNewLayout = false;

        Hovered = Visible && currentWindowBounds.ContainsPoint(viewport.MousePosition);
    }

    private void DoLayout(GUIViewport viewport, Vector2 predictedSize)
    {
        scopes.Clear();

        Rectangle windowBounds = new Rectangle(viewport.Bounds.GetAlignedPoint(Anchor), predictedSize, Alignment);

        Vector2 windowOrigin = windowBounds.Position + this.Offset;

        currentWindowBounds = new(windowOrigin, Vector2.Zero, this.Anchor);
        LastItemBounds = new(windowOrigin, Vector2.Zero);

        currentScope = new()
        {
            bounds = new(windowOrigin, Vector2.Zero),
            Cursor = windowOrigin,
            LayoutMode = LayoutMode.Column,
        };

        layout?.Invoke(this);
    }

    /// <summary>
    /// Shortcut for BeginScope(LayoutMode.Row). The GUIScope returned must be disposed to end the scope.
    /// </summary>
    public GUIScope Row()
    {
        BeginScope(LayoutMode.Row);
        return new(this, LayoutMode.Row);
    }

    /// <summary>
    /// Shortcut for BeginScope(LayoutMode.Column). The GUIScope returned must be disposed to end the scope
    /// </summary>
    public GUIScope Column()
    {
        BeginScope(LayoutMode.Column);
        return new(this, LayoutMode.Column);
    }

    public void BeginScope(LayoutMode mode)
    {
        scopes.Push(currentScope);

        currentScope = new()
        {
            LayoutMode = mode,
            Cursor = this.currentScope.Cursor,
            bounds = new(this.currentScope.Cursor, Vector2.Zero),
        };
    }

    public void EndScope(LayoutMode mode)
    {
        if (currentScope.LayoutMode != mode)
        {
            throw new InvalidOperationException("mismatching scope types");
        }

        // return to old scope and add ended one as an item to it
        LayoutScope endedScope = currentScope;
        currentScope = scopes.Pop();
        InsertItem(endedScope.bounds);
    }

    public void Text(string text, float size = 16, Color? color = null, TextStyle style = TextStyle.Regular)
    {
        Rectangle textBounds = Program.font.MeasureText(text, size);
        textBounds.Position += currentScope.Cursor + new Vector2(0, size);
        textBounds.X -= Margin;
        textBounds.Y -= Margin;
        textBounds.Width += Margin * 2;
        textBounds.Height += Margin * 2;

        AddCommand(new DrawCommand.Text(text, size, currentScope.Cursor + new Vector2(0, size), color, style));
        InsertItem(textBounds);
    }

    public void InsertItem(Rectangle itemBounds)
    {
        LastItemBounds = itemBounds;
        if (currentScope.LayoutMode == LayoutMode.Row)
        {
            currentScope.Cursor.X = LastItemBounds.X + LastItemBounds.Width;
        }
        else
        {
            currentScope.Cursor.Y = LastItemBounds.Y + LastItemBounds.Height;
        }
        this.currentWindowBounds = this.currentWindowBounds.Union(itemBounds);
        this.currentScope.bounds = this.currentScope.bounds.Union(itemBounds);

        if (ShowGUIItemBounds)
        {
            AddCommand(new DrawCommand.Rectangle(itemBounds, Color.Red, false));
        }
    }

    public bool TextButton(string text, float size = 16, bool disabled = false)
    {
        Rectangle textBounds = default;
        textBounds.Width = Program.font.MeasureText(text, size).Width;
        textBounds.Height = size;
        textBounds.Position = currentScope.Cursor + new Vector2(0, Margin);
        textBounds.X -= Margin;
        textBounds.Y -= Margin;
        textBounds.Width += Margin * 2;
        textBounds.Height += Margin * 2;
        
        Vector2 baseline = currentScope.Cursor + new Vector2(0, size);
         
        InsertItem(textBounds);
        
        if (!LastItemHovered())
        {
            AddCommand(new DrawCommand.Rectangle(textBounds, Color.FromHSV(.6f, .25f, .25f), true));
        }

        AddCommand(new DrawCommand.Rectangle(textBounds with { X = textBounds.X + 1, Y = textBounds.Y + 1 }, new Color(28, 33, 38), false));
        AddCommand(new DrawCommand.Rectangle(textBounds, new Color(70, 79, 89), false));
        
        if (LastItemHovered())
        {
            AddCommand(new DrawCommand.Rectangle(textBounds, Color.Gray, true));
        }

        AddCommand(new DrawCommand.Text(text, size, baseline));

        return LastItemClicked(MouseButton.Left);
    }

    public void Image(ITexture image)
    {
        Image(image, new(image.Width, image.Height));
    }

    public void Image(ITexture image, Vector2 size)
    {
        Rectangle bounds = new(currentScope.Cursor.X, currentScope.Cursor.Y, size.X, size.Y);
        InsertItem(bounds);
        AddCommand(new DrawCommand.Image(image, LastItemBounds));
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
        if (!Visible || commands.Count == 0)
        {
            return;
        }

        canvas.PushState();

        canvas.Fill(new Color(12, 17, 23));
        canvas.DrawRect(this.currentWindowBounds);

        canvas.Stroke(new Color(28, 33, 38));
        canvas.DrawRect(this.currentWindowBounds with { X = currentWindowBounds.X + 1, Y = currentWindowBounds.Y + 1 });

        canvas.Stroke(new Color(70, 79, 89));
        canvas.DrawRect(this.currentWindowBounds);

        foreach (var command in commands)
        {
            command.Render(canvas);
        }

        commands.Clear();
        canvas.PopState();
    }

    internal void ProgressBar(float progress, float width)
    {
        Rectangle bounds = new(currentScope.Cursor.X, currentScope.Cursor.Y, width, 5f);
        
        InsertItem(bounds);
        
        AddCommand(new DrawCommand.Rectangle(LastItemBounds, Color.Gray, true));
        AddCommand(new DrawCommand.Rectangle(LastItemBounds with { Width = LastItemBounds.Width * progress }, Color.DarkGray, true));
    }

    public void Separator()
    {
        Rectangle itemBounds = new(currentScope.Cursor.X, currentScope.Cursor.Y + Margin, this.predictedWindowBounds.Width - 2 * Margin, 1);
        // Cursor.Y += Margin;

        InsertItem(itemBounds);
        AddCommand(new DrawCommand.Rectangle(LastItemBounds, DefaultTextColor, true));
    }

    public void AddCommand(DrawCommand command)
    {
        if (Visible)
        {
            commands.Add(command);
        }
    }

    private struct LayoutScope
    {
        public Vector2 Cursor;
        public Rectangle bounds;
        public LayoutMode LayoutMode;
    }

}

/// <summary>
/// Helper class that calls window.EndScope(mode) when disposed.
/// </summary>
struct GUIScope(GUIWindow window, LayoutMode mode) : IDisposable
{
    public void Dispose()
    {
        window.EndScope(mode);
    }
}

abstract class DrawCommand
{
    public abstract void Render(ICanvas canvas);

    public class Text(string text, float size, Vector2 position, Color? color = null, TextStyle style = TextStyle.Regular) : DrawCommand
    {
        public override void Render(ICanvas canvas)
        {
            canvas.Font(Program.font);
            canvas.Fill(Color.FromHSV(0, 0, .05f));
            canvas.DrawText(text, size, position + new Vector2(1, 1), style);
            canvas.Fill(color ?? Color.FromHSV(0, 0, .65f));
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
    Column,
    Row,
}
