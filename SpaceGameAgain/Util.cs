using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal static class Util
{
    public static Vector2[] ToPolygon(this Rectangle rectangle)
    {
        return [
            rectangle.GetAlignedPoint(Alignment.TopLeft),
            rectangle.GetAlignedPoint(Alignment.TopRight),
            rectangle.GetAlignedPoint(Alignment.BottomRight),
            rectangle.GetAlignedPoint(Alignment.BottomLeft),
        ];
    }

    public static Vector2 Step(Vector2 point, Vector2 towards, float distance)
    {
        var delta = towards - point;
        if (delta.LengthSquared() < distance * distance)
        {
            return towards;
        }
        return point + delta.Normalized() * distance;
    }
    public static FixedVector2 Step(FixedVector2 point, FixedVector2 towards, float distance)
    {
        var delta = towards - point;
        if (delta.LengthSquared() < distance * distance)
        {
            return towards;
        }
        return point + delta.Normalized() * distance;
    }

    public static float? RayLineIntersect(Vector2 position, Vector2 direction, Vector2 from, Vector2 to)
    {
        var v1 = position - from;
        var v2 = to - from;
        var v3 = new Vector2(-direction.Y, direction.X);

        var dot = Vector2.Dot(v2, v3);
        if (Math.Abs(dot) < 0.0001f)
            return null;

        var t1 = Cross(v2, v1) / dot;
        var t2 = Vector2.Dot(v1, v3) / dot;

        if (t1 >= 0.0f && (t2 >= 0.0f && t2 <= 1.0f))
            return t1;

        return null;
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return Vector3.Cross(new(a, 0), new(b, 0)).Z;
    }

    public static bool TestPoint(Vector2[] polygon, Transform polygonTransform, Vector2 point, Transform pointTransform)
    {
        var localPoint = polygonTransform.WorldToLocal(pointTransform.LocalToWorld(point));

        var bounds = Polygon.GetBoundingRectangle(polygon);

        if (!bounds.ContainsPoint(localPoint))
            return false;

        int intersections = 0;
        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 from = polygon[i];
            Vector2 to = polygon[i + 1 >= polygon.Length ? 0 : (i + 1)];

            if (RayLineIntersect(localPoint, Vector2.UnitX, from, to) is not null)
            {
                intersections++;
            }
        }

        return intersections % 2 is 1;
    }
}