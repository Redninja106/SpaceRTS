using SimulationFramework.Drawing.Shaders;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SimulationFramework.Drawing.Shaders.ShaderIntrinsics;

namespace SpaceGame.Rendering;
internal class StarShader : CanvasShader
{
    private float size;
    private float rotation;

    public float Brightness = .333f;
    public float TurnSpeed = 0.0025f;
    public GalaxyInfo Galaxy = new();
    readonly ImmutableArray<float> noise = Enumerable.Range(0, 1024*8).Select(r => Random.Shared.NextSingle()).ToImmutableArray();

    public override ColorF GetPixelColor(Vector2 position)
    {
        float logSize = Log2(size);
        int minZoomLevel = (int)Floor(logSize);
        int maxZoomLevel = (int)Ceiling(logSize);

        ColorF minCol = GetColorAtZoom(position, minZoomLevel);
        ColorF maxCol = GetColorAtZoom(position, maxZoomLevel);

        return ColorF.Lerp(minCol, maxCol, logSize - minZoomLevel / (maxZoomLevel - minZoomLevel));
    }

    private ColorF GetColorAtZoom(Vector2 position, float zoomLevel)
    {
        const float densityExponent = 10f;

        float dist = position.Length();
        float angle = Atan2(position.Y, position.X);
        position = dist * Vec2(Cos(angle + rotation), Sin(angle + rotation));
        angle += dist / Galaxy.Radius * MathF.Tau * Galaxy.ArmCurveAmount;

        float edgeFactor = Clamp((Galaxy.Radius - dist) / Galaxy.armSize, 0, 1);
        float armFactor = (Galaxy.armSize - dist * Pow(ArmDistance(angle, Galaxy.armCount), Galaxy.armShapeFac)) / Galaxy.armSize;
        
        float secondaryEdgeFactor = Clamp(1 - Pow(Distance(Galaxy.Radius * .45f, dist) / (Galaxy.Radius * .45f), 2), 0, 1);
        float secondaryArmFactor = (Galaxy.armSize - dist * Pow(ArmDistance(angle + (1f / Galaxy.armCount * MathF.PI), Galaxy.armCount), Galaxy.armShapeFac)) / Galaxy.armSize;
        
        float density = MathF.Pow(edgeFactor * armFactor, densityExponent) + 
            MathF.Pow(secondaryEdgeFactor * secondaryArmFactor, densityExponent * 2);
        density *=.01f * zoomLevel * zoomLevel;

        float cellScale = Pow(2, zoomLevel) / 50f;
        Vector2 cellLoc = Mod(position, cellScale) / cellScale;
        int cellX = (int)Floor(position.X / cellScale);
        int cellY = (int)Floor(position.Y / cellScale);

        ColorF result = ColorF.Black;

        density = Clamp(density, 0, 1);
        float chance = Math.Min(density * Galaxy.starDensity, 1);
        for (int i = 0; i < density * Galaxy.starDensity; i++)
        {
            result += GetStarColor(cellLoc, Hash(cellX, cellY, i, (int)zoomLevel), chance);
        }

        return result;
    }

    float ArmDistance(float angle, int arms)
    {
        float armIdx = angle / MathF.Tau * arms;
        return Abs(armIdx - Round(armIdx));
    }

    int Hash(int x, int y, int z, int w)
    {
        int h = 17;

        h = h * 31 + x;
        h = h * 31 + y;
        h = h * 31 + z;
        h = h * 31 + w;

        h ^= (h >> 16);
        h *= 0x45d9f3b;
        h ^= (h >> 16);
        h *= 0x45d9f3b;
        h ^= (h >> 16);

        return h;
    }


    private float RandomTemperature(float b, float r)
    {
        float classO = 0.00003f * Pow(1400000, .3f);
        float classB = 0.0013f * Pow(20000, .3f);
        float classA = 0.006f * Pow(40, .3f);
        float classF = 0.03f * Pow(6, .3f);
        float classG = 0.076f * Pow(1.2f, .3f);
        float classK = 0.121f * Pow(0.4f, .3f);
        float classM = 0.7645f * Pow(0.04f, .3f);
        float totalWeight = classO + classB + classA + classF + classG + classK + classM;
        
        b *= totalWeight;

        if (b < classM)
        {
            return Lerp(2100, 3400, r);
        }
        if (b < classK)
        {
            return Lerp(3400, 4900, r);
        }
        if (b < classG)
        {
            return Lerp(4900, 5700, r);
        }
        if (b < classF)
        {
            return Lerp(5700, 7200, r);
        }
        if (b < classA)
        {
            return Lerp(7200, 9700, r);
        }
        if (b < classB)
        {
            return Lerp(9700, 30000, r);
        }
        //if (b < classO)
        //{
            return Lerp(30000, 60000, r);
        //}
    }

    private ColorF GetStarColor(Vector2 cellLoc, int rand, float chance)
    {
        int randomOffset = (rand % noise.Length);
        float offsetX = noise[randomOffset];
        float offsetY = noise[(randomOffset * rand % noise.Length)];
        float n = noise[(randomOffset * rand * rand % noise.Length)];
        float b = noise[(randomOffset * rand * rand * rand % noise.Length)];
        
        if (n > chance)
        {
            return ColorF.Black;
        }

        float brightness = 0.075f;
        float dist = Distance(Vec2(offsetX, offsetY), cellLoc);
        float rgb = Sqrt(Sqrt(brightness - dist));
        rgb = Clamp(rgb, 0, 1);
        float temp = RandomTemperature(b, n);
        Vector3 color = StarColor(temp);// * Log(1 + Pow(temp / 6500.0f, 4));
        return new ColorF(rgb * color.X, rgb * color.Y, rgb * color.Z) * Brightness;
    }

    private Vector3 StarColor(float temperature)
    {
        float t = temperature / 100.0f;
        Vector3 color = Vec3(0, 0, 0);

        // Red
        if (t <= 66)
        {
            color.X = 255;
        }
        else
        {
            color.X = 329.698727446f * Pow(t - 60f, -0.1332047592f);
        }

        // Green
        if (t <= 66)
        {
            color.Y = 99.4708025861f * Log(t) - 161.1195681661f;
        }
        else
        {
            color.Y = 288.1221695283f * Pow(t - 60, -0.0755148492f);
        }

        // Blue
        if (t >= 66)
        {
            color.Z = 255;
        }
        else if (t <= 19)
        {
            color.Z = 0;
        }
        else
        {
            color.Z = 138.5177312231f * Log(t - 10) - 305.0447927307f;
        }

        return Clamp(color, Vec3(0), Vec3(255)) / 255f;
    }

    // SimulationFramework doesn't expose this in shaders (yet!)
    private float Log2(float x)
    {
        return Log(x) / Log(2);
    }

    public void Render(ICanvas canvas, Camera camera)
    {
        canvas.PushState();
        canvas.ResetState();
        size = camera.SmoothVerticalSize;
        rotation = Time.TotalTime * TurnSpeed;
        TransformMatrix = camera.CreateRelativeMatrix(Transform.Default);
        canvas.Fill(this);
        canvas.DrawRect(0, 0, canvas.Width, canvas.Height);
        canvas.PopState();
    }
}

struct GalaxyInfo
{
    public float starDensity = 5;
    public float armSize = 15000;
    public float Radius = 35000;
    public float ArmCurveAmount = -.9f;
    public float armShapeFac = .8f;
    public int armCount = 2;

    public GalaxyInfo()
    {
    }
}