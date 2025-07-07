using Silk.NET.OpenGL;
using SimulationFramework.Drawing.Shaders;
using SpaceGame.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Silk.NET.Core.Native.WinString;
using static SimulationFramework.Drawing.Shaders.ShaderIntrinsics;

namespace SpaceGame.Planets;
internal class BlackHole : Planet
{
    BlackHoleShader shader = new();

    public BlackHole(PlanetPrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
    }

    public override void Render(ICanvas canvas)
    {
        shader.Radius = this.Radius;
        shader.Render(canvas);
    }
}

class BlackHolePrototype : PlanetPrototype
{
    public override Type ActorType => typeof(BlackHole);
}

class BlackHoleShader : CanvasShader
{
    public Vector2 effectPos;
    public Vector3 diskRotation = new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
    public Vector3 diskDrift = new(Random.Shared.NextSingle(-.005f, .005f), Random.Shared.NextSingle(-.005f, .005f), Random.Shared.NextSingle(-.005f, .005f));
    public Matrix4x4 diskInverseTransform;
    public float Radius;
    readonly ImmutableArray<float> noise = Enumerable.Range(0, 1024 * 8).Select(r => Random.Shared.NextSingle()).ToImmutableArray();
    public void Render(ICanvas canvas)
    {
        var diskRot = diskRotation * Time.TotalTime * diskDrift;
        diskInverseTransform = Matrix4x4.CreateRotationY(Time.TotalTime * .1f) * Matrix4x4.CreateFromYawPitchRoll(diskRot.Y, diskRot.X, diskRot.Z);
        Matrix4x4.Invert(diskInverseTransform, out diskInverseTransform);
        canvas.Fill(this);
        canvas.DrawCircle(0, 0, Radius * 10);
    }
    public override ColorF GetPixelColor(Vector2 position)
    {
        const float blackHoleRadius = 100;
        const float diskRadius = blackHoleRadius * 3;

        Vector3 rayDirection = new(0, 0, 1);
        Vector3 rayPosition = new(position.X, position.Y, -blackHoleRadius * 5);

        const float mass = .25f;

        // Vector2 uv = (position) / blackHoleRadius;
        // float theta = Atan(Length(uv));
        // float alpha = 4 * mass / Length(uv);
        // float beta = theta - alpha;
        // float into = Length(uv) / Tan(beta);
        // Vector3 newDir = Vec3(uv, Abs(into));

        ColorF color = new(0, 0, 0, 0);
        float holeDist = rayPosition.Length();
        for (int i = 0; i < 250; i++)
        {
            if (holeDist < blackHoleRadius)
            {
                color += new ColorF(0, 0, 0, holeDist > 0 ? 1 : 0);
                break;
            }

            Vector4 p = Vector4.Transform(new Vector4(rayPosition, 1), diskInverseTransform);
            float step = SdfCappedCylinder(p.GetXYZ(), 1f, diskRadius);
            if (step < 0.1f)
            {
                // float a = (Atan2(p.Z, p.X) + float.Pi) / float.Tau;// Length(Vec2(p.Z, p.X)) / (blackHoleRadius * 10);
                // float b = noise[(int)(a * 1234567) % 1024];
                // float r = noise[(int)(a * 1234567) % 1024];

                float b = 0;
                float w = 1;
                for (int j = 0; j < 4; j++)
                {
                    b += .25f * (1f/w) * noise[Hash((int)(p.X * w), (int)(p.Y * w), (int)(p.Z * w)) % 1024];
                    w *= .666f;
                }
                //b *= b;
                float alpha = (1f - (holeDist / diskRadius));
                float scale = b * alpha * alpha;
                color += new ColorF(1.2f * scale, 1.1f * scale, 1f * scale, alpha);
                step = .1f;
            }

            float bend = step * (1 / (holeDist * holeDist * holeDist)) * blackHoleRadius *1;
            rayDirection = Normalize(rayDirection - rayPosition * bend);
            rayPosition += rayDirection * step;
            holeDist = rayPosition.Length();
        }

        color /= color.A;
        return color;
    }

    public static int Hash(int x, int y, int z)
    {
        int h = 17;

        h = h * 31 + x;
        h = h * 31 + y;
        h = h * 31 + z;

        h ^= (h >> 16);
        h *= 0x45d9f3b;
        h ^= (h >> 16);
        h *= 0x45d9f3b;
        h ^= (h >> 16);

        return h;
    }

    float SdfCappedCylinder(Vector3 p, float h, float r)
    {
        Vector2 d = Abs(Vec2(Length(Vec2(p.X, p.Z)), p.Y)) - Vec2(r, h);
        return Min(Max(d.X, d.Y), 0.0f) + Length(Max(d, Vec2(0.0f)));
    }

    private float ClosestT(float ta, float tb)
    {
        if (ta < 0) return tb;
        if (tb < 0) return ta;
        float min = ShaderIntrinsics.Min(ta, tb);
        return min;
    }

    private float RaySphereIntersect(Vector3 rayOrigin, Vector3 rayDir, Vector3 sphereCenter, float sphereRadius)
    {
        Vector3 oc = rayOrigin - sphereCenter;
        float a = ShaderIntrinsics.Dot(rayDir, rayDir);
        float b = 2f * ShaderIntrinsics.Dot(oc, rayDir);
        float c = ShaderIntrinsics.Dot(oc, oc) - sphereRadius * sphereRadius;

        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
            return -1f;

        float sqrtD = ShaderIntrinsics.Sqrt(discriminant);
        float t1 = (-b - sqrtD) / (2 * a);
        float t2 = (-b + sqrtD) / (2 * a);

        if (t1 >= 0) return t1;
        if (t2 >= 0) return t2;

        return -1f;
    }

    public static float RayDiskIntersect(Vector3 rayOrigin, Vector3 rayDir, Vector3 diskCenter, Vector3 diskNormal, float diskRadius)
    {
        float denom = ShaderIntrinsics.Dot(diskNormal, rayDir);
        if (ShaderIntrinsics.Abs(denom) < 1e-6f)
            return -1f; // Ray is parallel to the disk

        float t = ShaderIntrinsics.Dot((diskCenter - rayOrigin), (diskNormal)) / denom;
        if (t < 0)
            return -1f; // Intersection is behind the ray origin

        Vector3 hitPoint = rayOrigin + rayDir * t;
        Vector3 toCenter = hitPoint - diskCenter;

        if (toCenter.LengthSquared() <= diskRadius * diskRadius)
            return t;

        return -1f;
    }
}