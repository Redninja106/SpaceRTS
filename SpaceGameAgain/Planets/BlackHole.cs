using Silk.NET.OpenGL;
using SimulationFramework.Drawing.Shaders;
using SimulationFramework.Drawing.Shaders.Compiler;
using SpaceGame.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static SimulationFramework.Drawing.Shaders.ShaderIntrinsics;

namespace SpaceGame.Planets;
internal class BlackHole : Planet
{
    BlackHoleShader shader = new();

    public BlackHole(PlanetPrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
        int count = 100_000;
        Vector3 color = Vector3.Zero;
        for (int i = 0; i < count; i++)
        {
            float t = GalaxyShader.RandomTemperature(Random.Shared.NextSingle(), Random.Shared.NextSingle());
            color += GalaxyShader.StarColor(t);
        }
        color /= count;
        Console.WriteLine(color);
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

static class NoiseTexture
{
    private static readonly Dictionary<(int, int), ITexture> textures = [];

    public static ITexture Get(int size = 1024, int blur = 0)
    {
        if (!textures.TryGetValue((size, blur), out ITexture? texture))
        {
            textures[(size, blur)] = texture = Create(size, blur);
        }

        return texture;
    }

    public static ITexture Create(int size, int blur = 0)
    {
        var texture = Graphics.CreateTexture(size, size);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                texture[x, y] = new Color(
                    (byte)Random.Shared.Next(0, 256),
                    (byte)Random.Shared.Next(0, 256),
                    (byte)Random.Shared.Next(0, 256),
                    (byte)Random.Shared.Next(0, 256)
                    );
            }
        }

        if (blur > 0)
        {
            int Wrap(int i)
            {
                return (i % size + size) % size;
            }

            float s = blur / 3f;
            float[] weights = new float[blur];
            float sum = 0;
            for (int i = 0; i < blur; i++)
            {
                float a = ((i - blur / 2f) / s);
                sum += weights[i] = MathF.Exp(-0.5f * a * a);
            }
            for (int i = 0; i < blur; i++)
            {
                weights[i] /= sum;
            }   
            
            Color[] blurredImage = new Color[size * size];
            // vertical blur
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float r = 0, g = 0, b = 0, a = 0;
                    for (int i = 0; i < blur; i++)
                    {
                        Color color = texture[x, Wrap(y + i - blur/2)];
                        r += color.R * weights[i];
                        g += color.G * weights[i];
                        b += color.B * weights[i];
                        a += color.A * weights[i];
                    }

                    blurredImage[y * size + x] = new(
                        (byte)(r),
                        (byte)(g),
                        (byte)(b),
                        (byte)(a)
                        );
                }
            }

            blurredImage.AsSpan().CopyTo(texture.Pixels);

            // horizontal blur
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float r = 0, g = 0, b = 0, a = 0;
                    for (int i = 0; i < blur; i++)
                    {
                        Color color = texture[Wrap(x + i - blur / 2), y];
                        r += color.R * weights[i];
                        g += color.G * weights[i];
                        b += color.B * weights[i];
                        a += color.A * weights[i];
                    }

                    blurredImage[y * size + x] = new(
                        (byte)(r),
                        (byte)(g),
                        (byte)(b),
                        (byte)(a)
                        );
                }
            }

            blurredImage.AsSpan().CopyTo(texture.Pixels);
        }

        texture.ApplyChanges();
        Graphics.GenerateMipmaps(texture);
        texture.Filter = TextureFilter.MipmapLinear;
        texture.WrapModeX = WrapMode.Repeat;
        texture.WrapModeY = WrapMode.Repeat;
        return texture;
    }
}

class BlackHoleShader : CanvasShader
{
    // TODO add lensing

    public Vector2 effectPos;
    public Vector3 diskRotation = new(Random.Shared.NextSingle() * float.Tau, Random.Shared.NextSingle() * float.Tau, Random.Shared.NextSingle() * float.Tau);
    public Vector3 diskDrift = new (Random.Shared.NextSingle(-.005f, .005f), Random.Shared.NextSingle(-.005f, .005f), Random.Shared.NextSingle(-.005f, .005f));
    public Matrix4x4 diskInverseTransform;
    public float Radius;
    private ITexture noiseTexture = NoiseTexture.Get();

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
        Vector3 rayPosition = new(position.X, position.Y, -blackHoleRadius * 50);

        const float minStep = .1f;
        const float diskThickness = 3;

        // Vector2 uv = (position) / blackHoleRadius;
        // float theta = Atan(Length(uv));
        // float alpha = 4 * mass / Length(uv);
        // float beta = theta - alpha;
        // float into = Length(uv) / Tan(beta);
        // Vector3 newDir = Vec3(uv, Abs(into));

        ColorF diskColor = new(0, 0, 0, 0);

        float holeCenterDist = rayPosition.Length();
        for (int i = 0; i < 500; i++)
        {
            if (holeCenterDist < blackHoleRadius)
            {
                // overwriting the alpha makes the black hole edge more defined
                diskColor *= diskColor.A;
                diskColor.A = 1;
                break;
            }

            if (rayPosition.Z > blackHoleRadius * 100)
            {
                // float squareSize = 25;
                // Vector3 projectedPosition = (rayDirection * (1f / rayDirection.Z) * (blackHoleRadius * 100 - rayPosition.Z));
                // float b = Mod(Round(rayPosition.X / squareSize) + Round(rayPosition.Y / squareSize), 2) == 0 ? 1 : 0;
                // diskColor += new ColorF(b, b, b, 1f);
                break;
            }

            Vector4 p = Vector4.Transform(new Vector4(rayPosition, 1), diskInverseTransform);
            float diskDistance = SdfCappedCylinder(p.GetXYZ(), diskThickness, diskRadius);
            float blackHoleDistance = holeCenterDist - blackHoleRadius;
            float step = float.Min(blackHoleDistance, diskDistance);
            if (diskDistance < minStep)
            {
                // average star colored noise for disk color
                const float brightness = 2f;
                ColorF n = noiseTexture.Sample(new(p.X, p.Z));
                ColorF noise = noiseTexture.Sample(new(p.Length(), p.Y));

                Vector3 pointColor = new Vector3(233f / 255f, 155f / 255f, 85f / 255f);

                Vector3 randomColor = GalaxyShader.StarColor(GalaxyShader.RandomTemperature(noise.R, noise.G));
                pointColor = Vector3.Lerp(pointColor, randomColor, n.B * .125f);

                pointColor *= brightness * Clamp((1f - (blackHoleDistance / (diskRadius - blackHoleRadius))), .30f, 1f);

                float density = (1.001f - (blackHoleDistance / (diskRadius - blackHoleRadius))) * Clamp(1 - (1 / (diskThickness * 2)) * Abs(p.Y), 0, 1);
                float absorption = density * minStep * 1;

                ColorF color = new ColorF(pointColor) with { A = absorption };

                diskColor.R += (1 - diskColor.A) * color.R * color.A;
                diskColor.G += (1 - diskColor.A) * color.G * color.A;
                diskColor.B += (1 - diskColor.A) * color.B * color.A;
                diskColor.A += (1 - diskColor.A) * color.A;

                if (diskColor.A > .999f)
                {
                    break;
                }
            }

            if (step < minStep)
            {
                step = minStep;
            }

            holeCenterDist = Max(holeCenterDist, minStep);
            float bend = step * (1 / (holeCenterDist * holeCenterDist * holeCenterDist)) * 50f;
            rayDirection = Normalize(rayDirection - rayPosition * bend);
            rayPosition += rayDirection * step;
            holeCenterDist = rayPosition.Length();
        }

        return diskColor;
    }

    float Noise(Vector3 p)
    {
        return noiseTexture.Sample(new(p.X, p.Z)).R;

        p = Fract(p * 0.3183099f + new Vector3(0.1f, 0.2f, 0.3f));
        p *= 17.0f;
        return Mod((p.X * p.Y * p.Z * (p.X + p.Y + p.Z)), 1.0f);
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