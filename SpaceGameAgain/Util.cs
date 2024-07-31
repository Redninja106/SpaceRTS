using SimulationFramework.SkiaSharp;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        var bounds = Polygon.GetBoundingBox(polygon);

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


    public static Vector2[] Triangulate(ReadOnlySpan<Vector2> polygon)
    {
        if (polygon.Length < 3)
        {
            return polygon.ToArray();
        }

        Vector2[] result = new Vector2[(polygon.Length - 2) * 3];
        Triangulate(polygon, result);
        return result;
    }

    public static bool IsClockwise(ReadOnlySpan<Vector2> polygon)
    {
        return SignedArea(polygon) > 0;
    }

    public static float Area(ReadOnlySpan<Vector2> polygon)
    {
        return MathF.Abs(SignedArea(polygon));
    }

    public static float SignedArea(ReadOnlySpan<Vector2> polygon)
    {
        // https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
        float result = 0;
        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 from = polygon[i];
            Vector2 to = polygon[i + 1 == polygon.Length ? 0 : i + 1];

            result += from.X * to.Y - to.X * from.Y;
        }

        return result * .5f;
    }

    public static void Triangulate(ReadOnlySpan<Vector2> polygon, Span<Vector2> triangles)
    {
        if (polygon.Length <= 3)
        {
            polygon.CopyTo(triangles);
            return;
        }

        List<Vector2> tris = [];
        int nextTri = 0;
        bool cw = IsClockwise(polygon);

        PolygonLinkedList list = polygon.Length < 512
            ? PolygonLinkedList.StackAllocate(polygon, stackalloc PolygonLinkedList.Vertex[polygon.Length])
            : PolygonLinkedList.HeapAllocate(polygon);

        int current;
        while (tris.Count < (polygon.Length - 2) * 3 && list.Length >= 3)
        {
            bool pushedTri = false;
            current = list.First();
            for (int i = 0; i < polygon.Length + 1; i++)
            {
                if (cw ^ IsVertexConvex(list, current))
                {
                    Vector2 p1 = list[current];
                    Vector2 p2 = list[list.Previous(current)];
                    Vector2 p3 = list[list.Next(current)];

                    int current2 = list.Next(list.Next(current));
                    bool pointInTri = false;
                    for (int j = 0; j < list.Length - 3; j++)
                    {
                        Debug.Assert(current2 != current);
                        Debug.Assert(current2 != list.Previous(current));
                        Debug.Assert(current2 != list.Next(current));

                        if (PointInTriangle(list[current2], p1, p2, p3))
                        {
                            pointInTri = true;
                            break;
                        }
                        current2 = list.Next(current2);
                    }

                    if (pointInTri)
                    {
                        current = list.Next(current);
                        continue;
                    }

                    triangles[nextTri++] = p1;
                    triangles[nextTri++] = p2;
                    triangles[nextTri++] = p3;

                    list.Remove(current);

                    if (list.Length < 3)
                    {
                        break;
                    }

                    pushedTri = true;
                }
                current = list.Next(current);
            }
            if (!pushedTri)
            {
                // error case: didn't find an ear
                return;
            }
        }

        static bool IsVertexConvex(PolygonLinkedList list, int index)
        {
            Vector2 vertex = list[index];
            Vector2 prev = list[list.Previous(index)];
            Vector2 next = list[list.Next(index)];

            Vector2 p1 = vertex - prev;
            Vector2 p2 = next - vertex;

            return (MathF.PI + MathF.Atan2(Cross(p1, p2), Vector2.Dot(p1, p2))) < MathF.PI;
        }
    }

    // https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
    public static bool PointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var s = (p0.X - p2.X) * (p.Y - p2.Y) - (p0.Y - p2.Y) * (p.X - p2.X);
        var t = (p1.X - p0.X) * (p.Y - p0.Y) - (p1.Y - p0.Y) * (p.X - p0.X);

        if ((s < 0) != (t < 0) && s != 0 && t != 0)
            return false;

        var d = (p2.X - p1.X) * (p.Y - p1.Y) - (p2.Y - p1.Y) * (p.X - p1.X);
        return d == 0 || (d < 0) == (s + t <= 0);
    }

    private ref struct PolygonLinkedList
    {
        Span<Vertex> vertices;
        int firstVertex;
        int length;

        public int Length => length;

        public static PolygonLinkedList HeapAllocate(ReadOnlySpan<Vector2> polygon)
        {
            PolygonLinkedList result = new();

            result.vertices = new Vertex[polygon.Length];
            result.Initialize(polygon);

            return result;
        }

        public static PolygonLinkedList StackAllocate(ReadOnlySpan<Vector2> polygon, Span<Vertex> stackMemory)
        {
            Debug.Assert(stackMemory.Length == polygon.Length);

            PolygonLinkedList result = new();

            result.vertices = stackMemory;
            result.Initialize(polygon);

            return result;
        }

        private void Initialize(ReadOnlySpan<Vector2> polygon)
        {
            for (int i = 0; i < polygon.Length; i++)
            {
                vertices[i].position = polygon[i];
                vertices[i].prev = i - 1;
                vertices[i].next = i + 1;
            }
            vertices[0].prev = polygon.Length - 1;
            vertices[polygon.Length - 1].next = 0;

            length = polygon.Length;
        }


        public Vector2 this[int index] => vertices[index].position;

        public void Remove(int index)
        {
            var vertex = vertices[index];

            if (index == firstVertex)
            {
                firstVertex = Next(firstVertex);
            }

            vertices[vertex.next].prev = vertex.prev;
            vertices[vertex.prev].next = vertex.next;

            length--;
        }

        public int Next(int index)
        {
            return vertices[index].next;
        }

        public int Previous(int index)
        {
            return vertices[index].prev;
        }

        public int First()
        {
            return firstVertex;
        }

        public struct Vertex
        {
            public Vector2 position;
            public int next;
            public int prev;
        }
    }

    public static unsafe void WriteValue<T>(this Stream stream, T value) where T : unmanaged
    {
        stream.Write(new Span<byte>(Unsafe.AsPointer(ref value), Unsafe.SizeOf<T>()));
    }

    public static unsafe T ReadValue<T>(this Stream stream) where T : unmanaged
    {
        T result = default;
        var span = new Span<byte>(Unsafe.AsPointer(ref result), Unsafe.SizeOf<T>());
        stream.Read(span);
        return result;
    }

    public static unsafe void WriteString(this Stream stream, string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        stream.WriteValue(bytes.Length);
        stream.Write(bytes);
    }

    public static unsafe string ReadString(this Stream stream)
    {
        var length = stream.ReadValue<int>();
        byte[] bytes = new byte[length];
        stream.Read(bytes);
        return Encoding.UTF8.GetString(bytes);
    }

    public static unsafe void DrawTriangles(this ICanvas canvas, ReadOnlySpan<Vector2> triangles)
    {
        var skcanvas = SkiaInterop.GetCanvas(canvas);
        var paint = SkiaInterop.GetPaint(canvas.State);
        nint vertices;
        fixed (Vector2* tris = triangles) 
        {
            vertices = sk_vertices_make_copy(SKVertexMode.Triangles, triangles.Length, (SKPoint*)tris, null, null, 0, null);
        }

        sk_canvas_draw_vertices(skcanvas.Handle, vertices, SKBlendMode.Plus, paint.Handle);
        sk_vertices_unref(vertices);
    }

    [DllImport("libSkiaSharp")]
    private static unsafe extern nint sk_vertices_make_copy(SKVertexMode vertexMode, int vertexCount, SKPoint* positions, SKPoint* texs, SKColor* colors, int indexCount, ushort* indices);
    [DllImport("libSkiaSharp")]
    private static unsafe extern void sk_canvas_draw_vertices(nint canvas, nint vertices, SKBlendMode mode, nint paint);
    [DllImport("libSkiaSharp")]
    private static unsafe extern void sk_vertices_unref(nint cvertices);

    // https://stackoverflow.com/questions/27939882/fast-crc-algorithm
    public static uint CRC(uint crc, ReadOnlySpan<byte> data)
    {
        const uint poly = 0x82f63b78;
        for (int i = 0; i < data.Length; i++)
        {
            crc ^= data[i];
            for (int k = 0; k < 8; k++)
                crc = (crc & 1u) == 0 ? (crc >> 1) ^ poly : crc >> 1;
        }
        return crc;
    }

    public static uint CRC<T>(uint crc, T value)
        where T : unmanaged
    {
        return CRC(crc, MemoryMarshal.AsBytes(new ReadOnlySpan<T>(ref value)));
    }

}
