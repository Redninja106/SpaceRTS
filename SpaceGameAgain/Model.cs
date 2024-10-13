using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class Model
{
    public Shape[] Shapes { get; }

    public Model(Vector2[] vertices, Color color) : this([new PolygonShape(vertices, color)])
    {
    }

    public Model(Shape[] shapes)
    {
        this.Shapes = shapes;
    }

    public void Render(ICanvas canvas, RenderParameters parameters)
    {
        foreach (var shape in Shapes)
        {
            canvas.PushState();
            shape.Render(canvas, parameters);
            canvas.PopState();
        }
    }

    public void RenderShadow(ICanvas canvas, Vector2 offset)
    {
        foreach (var shape in Shapes)
        {
            canvas.PushState();
            shape.RenderShadow(canvas, offset, Color.Black with { A = 100 });
            canvas.PopState();
        }
    }


    public void Render(ICanvas canvas, byte? alpha = null, Color? color = null)
    {
        Render(canvas, new()
        {
            alphaOverride = alpha,
            colorOverride = color,
        });
    }
}

struct RenderParameters
{
    public Color? colorOverride;
    public byte? alphaOverride;

    public Color GetOverriddenColor(Color defaultColor)
    {
        return (colorOverride ?? defaultColor) with { A = alphaOverride ?? colorOverride?.A ?? defaultColor.A };
    }
}

internal abstract class Shape
{
    public abstract void Render(ICanvas canvas, RenderParameters parameters);
    public virtual void RenderShadow(ICanvas canvas, Vector2 direction, Color shadowColor)
    {
    }
}

internal class PolygonShape : Shape
{
    public Vector2[] Vertices;
    public Color Color;
    public float height;

    public PolygonShape(Vector2[] vertices, Color color, float height = 0)
    {
        Vertices = vertices;
        Color = color;
        this.height = height;
    }

    public override void Render(ICanvas canvas, RenderParameters parameters)
    {
        canvas.Fill(parameters.GetOverriddenColor(Color.Gray));
        canvas.DrawPolygon(Vertices);
        canvas.Translate(0, -height);
        canvas.Fill(parameters.GetOverriddenColor(Color));
        canvas.DrawPolygon(Vertices);
    }

    public override void RenderShadow(ICanvas canvas, Vector2 offset, Color shadowColor)
    {
        canvas.Fill(shadowColor);

        ShadowVertexWriter writer = new(stackalloc Vector2[this.Vertices.Length * 2]);
        ProjectVerts(Vertices, offset, ref writer);
        canvas.DrawPolygon(writer.GetBuffer());

        base.RenderShadow(canvas, offset, shadowColor);
    }

    public static void RenderShadowPolygon(ICanvas canvas, Vector2[] polygon, Vector2 offset)
    {
        ShadowVertexWriter writer = new(stackalloc Vector2[polygon.Length * 2]);
        ProjectVerts(polygon, offset, ref writer);
        canvas.DrawPolygon(writer.GetBuffer());
    }

    public static void ProjectVerts(Vector2[] verts, Vector2 offset, ref ShadowVertexWriter writer)
    {
        var offsetAngle = Angle.FromVector(offset);
        int startVertex = 0;
        while (startVertex < verts.Length * 2 && InShadow(verts, startVertex, offsetAngle))
        {
            startVertex++;
        }

        for (int i = startVertex; i < verts.Length + startVertex; i++)
        {
            if (InShadow(verts, i, offsetAngle))
            {
                writer.Write(verts[Wrap(i, verts.Length)], verts[Wrap(i, verts.Length)] + offset);
            }
        }

        static bool InShadow(Vector2[] vertices, int vertex, float offsetAngle)
        {

            Vector2 previousVertex = vertices[Wrap(vertex - 1, vertices.Length)];
            Vector2 currentVertex = vertices[Wrap(vertex, vertices.Length)];
            Vector2 nextVertex = vertices[Wrap(vertex + 1, vertices.Length)];

            float fromAngle = Angle.FromVector(previousVertex - currentVertex);
            float toAngle = Angle.FromVector(nextVertex - currentVertex);


            bool invertRange = MathF.Abs(toAngle - fromAngle) > MathF.PI;
            return invertRange ^ !(offsetAngle > MathF.Min(toAngle, fromAngle) && offsetAngle < MathF.Max(toAngle, fromAngle));
        }

        static int Wrap(int value, int length)
        {
            if (value < 0)
            {
                return value % length + length;
            }
            else
            {
                return value % length;
            }
        }
    }

    private float AngleBetween(Vector2 a, Vector2 b)
    {
        return Angle.FromVector(b) - Angle.FromVector(a);
    }

    public ref struct ShadowVertexWriter(Span<Vector2> buffer)
    {
        private int outerPosition = buffer.Length / 2;
        private int innerPosition = buffer.Length / 2 - 1;
        private readonly Span<Vector2> buffer = buffer;

        public void Write(Vector2 vertex, Vector2 projectedVertex)
        {
            buffer[innerPosition--] = vertex;
            buffer[outerPosition++] = projectedVertex;
        }

        public readonly Span<Vector2> GetBuffer()
        {
            return buffer[(innerPosition+1)..outerPosition];
        }
    }

}

internal class CanvasShape : Shape
{
    public Action<ICanvas, RenderParameters>? RenderAction;
    public Action<ICanvas, Vector2, Color>? RenderShadowAction;

    public CanvasShape(Action<ICanvas, RenderParameters> renderAction, Action<ICanvas, Vector2, Color>? renderShadowAction = null)
    {
        RenderAction = renderAction;
        this.RenderShadowAction = renderShadowAction;
    }

    public override void Render(ICanvas canvas, RenderParameters parameters)
    {
        RenderAction?.Invoke(canvas, parameters);
    }

    public override void RenderShadow(ICanvas canvas, Vector2 direction, Color shadowColor)
    {
        RenderShadowAction?.Invoke(canvas, direction, shadowColor);
    }
}