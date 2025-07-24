using SpaceGame.Interaction;
using SpaceGame.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;

public delegate void GUILayout(GUIWindow window);

public sealed class GUIWindow
{
    public static Color DefaultTextColor = Color.FromHSV(0, 0, .65f);
    public static float CornerRadius = 10f;

    public bool RenderBackground = true;

    public Vector2 Offset = new(0, 0);
    public Alignment Anchor = Alignment.TopLeft;
    public Alignment Alignment = Alignment.TopLeft;

    public bool Visible = true;
    public bool Hovered = false;
    public bool DummyLayout = false;

    public Rectangle LastItemBounds;

    private Stack<LayoutScope> scopes = [];
    private LayoutScope currentScope;

    private Rectangle predictedWindowBounds;
    private Rectangle currentWindowBounds;
    private List<DrawCommand> commands = [];

    private Vector2 mousePosition;

    public float Margin = 3;

    private GUILayout? layout;
    
    public bool HasNewLayout;
    public GUILayout? Layout => layout;
    public GUIViewport Viewport { get; set; }

    [DebugOverlay]
    public static bool ShowGUIItemBounds;


    public GUIWindow(GUILayout? layout)
    {
        SetLayout(layout);
    }

    public Vector2 GetViewportMousePosition()
    {
        return mousePosition;
    }

    public Vector2 GetLocalMousePosition()
    {
        return mousePosition - this.Offset;
    }

    public Vector2 GetLastItemMousePosition()
    {
        return mousePosition - this.LastItemBounds.Position;
    }

    public void SetLayout(GUILayout? layout)
    {
        if (this.layout != layout) 
        {
            this.layout = layout;
            HasNewLayout = layout != null;
        }
    }

    public void Update()
    {
        commands.Clear();

        mousePosition = Viewport.MousePosition;

        // finalWindowBounds = currentWindowBounds;

        if (layout != null)
        {
            // do an invisible layout to determine approximate window size
            DummyLayout = true;
            DoLayout(Viewport, Vector2.Zero);
            DummyLayout = false;
            predictedWindowBounds = currentWindowBounds;
        }

        DoLayout(Viewport, predictedWindowBounds.Size);
        HasNewLayout = false;

        Hovered = Visible && currentWindowBounds.ContainsPoint(Viewport.MousePosition);
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
            Cursor = windowOrigin + new Vector2(Margin, Margin),
            LayoutMode = LayoutMode.Column,
        };

        layout?.Invoke(this);

        this.currentWindowBounds.Size += new Vector2(Margin, Margin);
    }

    /// <summary>
    /// Shortcut for <c>BeginScope(LayoutMode.Row)</c>. The GUIScope returned must be disposed to end the scope.
    /// </summary>
    public GUIScope Row()
    {
        BeginScope(LayoutMode.Row);
        return new(this, LayoutMode.Row);
    }

    /// <summary>
    /// Shortcut for <c>BeginScope(LayoutMode.Column)</c>. The GUIScope returned must be disposed to end the scope
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

        Rectangle textArea = new()
        {
            Position = currentScope.Cursor,
            Size = textBounds.Size + new Vector2(Margin * 2)
        };

        Vector2 baseline = currentScope.Cursor + new Vector2(Margin) - textBounds.Position;

        AddCommand(new DrawCommand.Text(text, size, baseline, color, style));
        InsertItem(textArea);
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

    public bool TextButton(string text, float size = 16, bool disabled = false, bool fitArea = false, bool centerText = false)
    {
        Rectangle textBounds = Program.font.MeasureText(text, size);
        Rectangle itemBounds = new()
        {
            Position = currentScope.Cursor,
            Size = textBounds.Size + new Vector2(Margin * 4)
        };

        Rectangle buttonBounds = new()
        {
            Position = currentScope.Cursor + new Vector2(Margin),
            Size = textBounds.Size + new Vector2(Margin * 2),
        };

        if (fitArea)
        {
            float localX = buttonBounds.X - predictedWindowBounds.Position.X;
            buttonBounds.Width = predictedWindowBounds.Width - Margin * 4;
            itemBounds.Width = predictedWindowBounds.Width - Margin * 2;
        }

        Vector2 baseline = currentScope.Cursor + new Vector2(Margin * 2) - textBounds.Position;
        
        if (centerText)
        {
            Rectangle bounds = Program.font.MeasureText(text, size);
            baseline.X += (buttonBounds.Width - bounds.Width) / 2f;
        }


        InsertItem(itemBounds);
        
        if (!LastItemHovered())
        {
            AddCommand(new DrawCommand.Rectangle(buttonBounds, Color.FromHSV(.6f, .25f, .25f), true));
        }

        AddCommand(new DrawCommand.Rectangle(buttonBounds with { X = buttonBounds.X + 1, Y = buttonBounds.Y + 1 }, new Color(28, 33, 38), false));
        AddCommand(new DrawCommand.Rectangle(buttonBounds, new Color(70, 79, 89), false));
        
        if (LastItemHovered())
        {
            AddCommand(new DrawCommand.Rectangle(buttonBounds, Color.Gray, true));
        }

        AddCommand(new DrawCommand.Text(text, size, baseline));

        return AreaClicked(buttonBounds, MouseButton.Left);
    }

    public void Image(ITexture image, bool inline = false, ColorF? tint = null)
    {
        Image(image, new(image.Width, image.Height), inline, tint);
    }

    public void Image(ITexture image, Vector2 size, bool inline = false, ColorF? tint = null)
    {
        Vector2 margin = inline ? Vector2.Zero : new Vector2(Margin);
        Rectangle itemBounds = new(currentScope.Cursor, size + margin * 2);
        Rectangle imageBounds = new(itemBounds.Position + margin, size);

        //Rectangle bounds = new(currentScope.Cursor.X, currentScope.Cursor.Y, size.X, size.Y);
        InsertItem(itemBounds);
        AddCommand(new DrawCommand.Image(image, imageBounds, tint ?? ColorF.White));
    }

    public bool AreaHovered(Rectangle area)
    {
        return !DummyLayout && Visible && area.ContainsPoint(mousePosition);
    }

    public bool AreaClicked(Rectangle area, MouseButton button)
    {
        return AreaHovered(area) && Mouse.IsButtonPressed(button);
    }

    public bool AreaHeld(Rectangle area, MouseButton button)
    {
        return AreaHovered(area) && Mouse.IsButtonDown(button);
    }

    public bool LastItemHovered()
    {
        return AreaHovered(LastItemBounds);
    }

    public bool LastItemClicked(MouseButton button)
    {
        return AreaClicked(LastItemBounds, button);
    }

    public bool LastItemHeld(MouseButton button)
    {
        return AreaHeld(LastItemBounds, button);
    }

    public void Render(ICanvas canvas, float displayWidth, float displayHeight)
    {
        if (!Visible || commands.Count == 0)
        {
            return;
        }

        canvas.PushState();

        if (RenderBackground)
        {
            canvas.Fill(new Color(12, 17, 23));
            canvas.DrawRoundedRect(this.currentWindowBounds, CornerRadius);

            canvas.Stroke(new Color(28, 33, 38));
            canvas.DrawRoundedRect(this.currentWindowBounds with { X = currentWindowBounds.X + 1, Y = currentWindowBounds.Y + 1 }, CornerRadius);

            canvas.Stroke(new Color(70, 79, 89));
            canvas.DrawRoundedRect(this.currentWindowBounds, CornerRadius);
        }

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
        
        AddCommand(new DrawCommand.Rectangle(LastItemBounds, new Color(0x40, 0x40, 0x40, 0xFF), true));
        AddCommand(new DrawCommand.Rectangle(LastItemBounds with { Width = LastItemBounds.Width * progress }, new Color(0x80, 0x80, 0x80, 0xFF), true));
    }

    public void Separator(Color? color = null)
    {
        Rectangle separatorBounds;
        Rectangle itemBounds;

        if (currentScope.LayoutMode == LayoutMode.Column)
        {
            separatorBounds = new(currentScope.Cursor.X, currentScope.Cursor.Y + Margin, this.predictedWindowBounds.Width - Margin - (currentScope.Cursor.X - currentWindowBounds.X), 1);
            itemBounds = new(currentScope.Cursor.X, currentScope.Cursor.Y + Margin, 0, 1 + Margin);
        }
        else if (currentScope.LayoutMode == LayoutMode.Row)
        {
            separatorBounds = new(currentScope.Cursor.X + Margin, currentScope.Cursor.Y, 1, this.predictedWindowBounds.Height - 2 * Margin);
            itemBounds = new(currentScope.Cursor.X + Margin, currentScope.Cursor.Y, 1 + Margin, 0);
        }
        else
        {
            throw new NotSupportedException();
        }

        InsertItem(itemBounds);
        AddCommand(new DrawCommand.Rectangle(separatorBounds, color ?? DefaultTextColor, true));
    }

    public Vector2 GetPosition()
    {
        return Viewport.Bounds.GetAlignedPoint(this.Anchor) + Offset;
    }

    public void AddCommand(DrawCommand command)
    {
        if (!DummyLayout && Visible)
        {
            commands.Add(command);
        }
    }

    public Vector2 GetPredictedSize()
    {
        return predictedWindowBounds.Size;
    }

    public Rectangle GetPredictedBounds()
    {
        return predictedWindowBounds;
    }

    internal void ModelImage(SpriteModel model, Vector2 size)
    {
        Rectangle itemBounds = new(currentScope.Cursor, size + new Vector2(Margin * 2));
        Rectangle imageBounds = new(itemBounds.Position + new Vector2(Margin), size);

        //Rectangle bounds = new(currentScope.Cursor.X, currentScope.Cursor.Y, size.X, size.Y);
        InsertItem(itemBounds);
        AddCommand(new DrawCommand.Model(model, imageBounds.GetAlignedPoint(Alignment.Center), size));
    }

    public void MinSize(Vector2 size)
    {
        if (this.currentWindowBounds.Width < size.X)
        {
            this.currentWindowBounds.Width = size.X;
        }
        if (this.currentWindowBounds.Height < size.Y)
        {
            this.currentWindowBounds.Height = size.Y;
        }
    }

    private struct LayoutScope
    {
        public Vector2 Cursor;
        public Rectangle bounds;
        public LayoutMode LayoutMode;
    }

}
