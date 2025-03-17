using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
public static class DebugDraw
{
    private static List<(Vector2[], Color color, Transform transform)> polygons = [];
    private static List<((Vector2, Vector2), Color color, Transform transform)> lines = [];
    private static List<(Circle, Color color, Transform transform)> circles = [];
    private static List<(Rectangle, Color color, Transform transform)> rectangles = [];

    public static void Polygon(Vector2[] polygon, Transform? transform = null, Color? color = null)
    {
        polygons.Add((polygon, color ?? Color.Red, transform ?? Transform.Default));
    }

    public static void Line(Vector2 from, Vector2 to, Transform? transform = null, Color? color = null)
    {
        lines.Add(((from, to), color ?? Color.Red, transform ?? Transform.Default));
    }

    public static void Rectangle(Rectangle rect, Transform? transform = null, Color? color = null)
    {
        rectangles.Add((rect, color ?? Color.Red, transform ?? Transform.Default));
    }

    public static void Clear()
    {
        lines.Clear();
        polygons.Clear();
        circles.Clear();
        rectangles.Clear();
    }

    public static void Draw(ICanvas canvas, Camera camera)
    {
        canvas.ResetState();
        canvas.PushState();
        canvas.StrokeWidth(0);

        foreach (var (poly, color, t) in polygons)
        {
            canvas.PushState();
            t.ApplyTo(canvas, camera);

            canvas.Stroke(color);
            canvas.DrawPolygon(poly);

            canvas.PopState();
        }

        foreach (var ((from, to), color, t) in lines)
        {
            canvas.PushState();
            t.ApplyTo(canvas, camera);

            canvas.Stroke(color);
            canvas.DrawLine(from, to);

            canvas.PopState();
        }

        foreach (var (circle, color, t) in circles)
        {
            canvas.PushState();
            t.ApplyTo(canvas, camera);

            canvas.Stroke(color);
            canvas.DrawCircle(circle);

            canvas.PopState();
        }

        foreach (var (rect, color, t) in rectangles)
        {
            canvas.PushState();
            t.ApplyTo(canvas, camera);

            canvas.Stroke(color);
            canvas.DrawRect(rect);

            canvas.PopState();
        }

        canvas.PopState();
    }

    public static void Circle(Vector2 position, float radius, Transform? transform = null, Color? color = null)
    {
        Circle(new(position, radius), transform, color);
    }

    public static void Circle(Circle circle, Transform? transform = null, Color? color = null)
    {
        circles.Add((circle, color ?? Color.Red, transform ?? Transform.Default));
    }

    public static void Ray(Vector2 origin, Vector2 direction, Transform? transform = null, Color? color = null)
    {
        Line(origin, origin + direction, transform, color);
    }
}
