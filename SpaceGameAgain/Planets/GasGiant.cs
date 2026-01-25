using SimulationFramework.Drawing.Shaders;
using SimulationFramework.Drawing.Shaders.Compiler;
using SpaceGame.Rendering;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SimulationFramework.Drawing.Shaders.ShaderIntrinsics;
namespace SpaceGame.Planets;
internal class GasGiant : Planet
{
    public override GasGiantPrototype Prototype => (GasGiantPrototype)base.Prototype;
    
    public GasGiantParameters Parameters { get; set; }

    GasGiantShader shader = new();
    RingShader? ringShader;

    public GasGiant(GasGiantPrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
        if (prototype.Randomize)
        {
            Parameters = GasGiantParameters.Randomize(Random.Shared);
        }
        else
        {
            this.Parameters = prototype.Parameters;
        }

        if (Parameters.HasRings)
        {
            ringShader = new();
        }
    }

    public override void Render(ICanvas canvas)
    {
        ringShader?.Setup(this);
        
        if (ringShader != null) 
        {
            canvas.Fill(ringShader);
            canvas.DrawArc(0, 0, this.Radius * RingShader.Width, this.Radius * RingShader.Height, MathF.PI, MathF.Tau, true, Alignment.Center);
        }

        shader.Setup(this);
        canvas.Fill(shader);
        canvas.DrawCircle(0, 0, Radius);

        if (ringShader != null)
        {
            canvas.Fill(ringShader);
            canvas.DrawArc(0, 0, this.Radius * RingShader.Width, this.Radius * RingShader.Height, 0, MathF.PI, true, Alignment.Center);
        }

        SphereOfInfluence.Render(canvas);
    }
}

class GasGiantPrototype : PlanetPrototype
{
    public override Type ActorType => typeof(GasGiant);
    
    public GasGiantParameters Parameters { get; set; }
    
    public bool Randomize { get; set; } = false;

    public override Planet CreateActor(GameWorld world, ulong id)
    {
        return base.CreateActor(world, id);
    }
}

class GasGiantShader : CanvasShader
{
    float radius;
    Vector3 lightDir;
    ITexture noise = NoiseTexture.Get(256, 7);
    ColorF[] colors = [];
    float time;

    float vorticity = 1;

    public override ColorF GetPixelColor(Vector2 position)
    {
        Vector2 dir = position / radius;
        float h = MathF.Sqrt(1f - dir.LengthSquared());
        float weakH = MathF.Sqrt(1f - .6f * dir.LengthSquared());
        Vector3 surfaceNormal = new(dir.X, dir.Y, h);

        Vector2 p = position * (1f / weakH) / (radius * .5f);
        
        float brightness = NormalMapHelper.CalcBrightness(surfaceNormal, lightDir);
        ColorF color = SurfaceColor(p);
        
        color *= brightness;
        color.A = 1;
        return color;
    }

    private ColorF SurfaceColor(Vector2 p)
    {
        //return noise.SampleUV(p) with { A = 1 };
        //p.X += p.Y * 0.1f;

        const int iterations = 6;
        const float e = 0.1f;
        p.X += 0.25f * (0.5f * noise.SampleUV(new(p.Y, 0)).B - .25f);
        p.X += time * 0.01f;

        for (int i = 0; i < iterations; i++)
        {
            float dx = (2f/e) * (NoiseSample(p.X + e, p.Y) - NoiseSample(p.X - e, p.Y));
            float dy = (2f/e) * (NoiseSample(p.X, p.Y + e) - NoiseSample(p.X, p.Y - e));
            p += new Vector2(dy, -dx) * 0.01f * (1 - i / (float)(iterations)) * vorticity;
        }

        return BaseGradient(p);
    }

    private float NoiseSample(float x, float y)
    {
        const float flowSpeed = 0.05f;

        ColorF sample1 = noise.SampleUV(new(x, y));
        float ti = Mod(time * flowSpeed, 4f);
        float t = Fract(time * flowSpeed);
        if (ti < 1) return Lerp(sample1.R, sample1.G, t);
        if (ti < 2) return Lerp(sample1.G, sample1.B, t);
        if (ti < 3) return Lerp(sample1.B, sample1.A, t);
        else return Lerp(sample1.A, sample1.R, t);
        // return ShaderIntrinsics.Lerp(sample1.R, sample1.G, ShaderIntrinsics.Fract(time));
    }

    private ColorF BaseGradient(Vector2 p)
    {
        float t = Fract((MathF.Sin(p.X * 10f + time) * 0.01f + p.Y) * .5f + .5f);
        int index = (int)(t * colors.Length);
        ColorF from = colors[index];
        ColorF to = colors[(index + 1) % colors.Length];
        ColorF gradientColor = ColorF.Lerp(from, to, SmoothStep(0, 1, Fract(t * colors.Length)));

        float noiseColor = noise.SampleUV(p).G;
        float brightness = 1 - (MathF.Pow(noiseColor, 4f) * vorticity);

        return gradientColor * brightness;
    }

    public static float SmoothStep(float a, float b, float t)
    {
        float num = Clamp((t - a) / (b - a), 0f, 1f);
        return num * num * (3f - 2f * num);
    }

    public void Setup(GasGiant planet)
    {
        radius = planet.Radius;
        colors = planet.Parameters.Colors;
        vorticity = planet.Parameters.Vorticity;
        noise.Filter = TextureFilter.Linear;
        time = Time.TotalTime;

        Star? star = planet.World.GetStar(planet.InterpolatedTransform.Position);
        if (star != null)
        {
            Vector2 v = (planet.Transform.Position - star.Transform.Position).ToVector2().Normalized();
            lightDir = new Vector3(v.X, v.Y, -1).Normalized();
            // tint = ColorF.Lerp(ColorF.White, star.Prototype.Color, .2f);
        }
    }
}

class RingShader : CanvasShader
{
    public const float Width = 2.1f;
    public const float Height = 0.35f;

    float radius;
    Vector3 lightDir;
    
    ITexture noiseTexture = NoiseTexture.Get();

    public override ColorF GetPixelColor(Vector2 position)
    {
        Vector2 p = position / (radius * new Vector2(Width, Height));
        float d = p.Length();

        const float innerEdge = 0.5f;

        if (d < innerEdge)
        {
            Discard();
            return default;
        }

        float edge = Sqrt(5*Min(d - innerEdge, 1f - d));
        
        float low = noiseTexture.SampleUV(new(d*0.1f, 0)).R;
        float high = noiseTexture.SampleUV(new(d * 0.5f, 0)).R;

        float density = Pow(Lerp(low, high, 0.25f), .6f);
        Vector2 l = new Vector2(lightDir.X, -Abs(lightDir.Y)).Normalized();
        Vector2 sp = p + (1 - 1.4f) * l * Dot(p, l);
        //sp.Y = -Abs(sp.Y);
        // sp.X += lightDir.Y;
        // sp.Y += lightDir.X;
        float sd = sp.Length();
        float shadow = 1f;
        if (sd < 1f/Width && Dot(sp, new Vector2(lightDir.X, -Abs(lightDir.Y))) > 0)
        {
            shadow = NormalMapHelper.AmbientLight;
        }

        ColorF baseColor = new ColorF(121 / 255F, 115 / 255F, 97 / 255F);
        ColorF color = baseColor * density * shadow;
        color.A = edge;
        return color;
    }

    internal void Setup(GasGiant planet)
    {
        radius = planet.Radius; 
        
        Star? star = planet.World.GetStar(planet.InterpolatedTransform.Position);
        if (star != null)
        {
            Vector2 v = (planet.Transform.Position - star.Transform.Position).ToVector2().Normalized();
            lightDir = new Vector3(v.X, v.Y, -1).Normalized();
            // tint = ColorF.Lerp(ColorF.White, star.Prototype.Color, .2f);
        }
    }
}

struct GasGiantParameters
{
    public GasGiantParameters()
    {
    }

    public ColorF[] Colors { get; set; } = [];
    public float Vorticity { get; set; } = 1;
    public bool HasRings { get; set; } = false;

    public static GasGiantParameters Randomize(Random random)
    {
        GasGiantParameters parameters = default;

        const float variance = .2f;

        float baseHue = random.NextSingle();
        float baseSat = random.NextSingle(0f, .5f);
        float baseVal = random.NextSingle(.6f, .8f);
        int colorCount = random.Next(1, 13);

        parameters.Colors = new ColorF[colorCount];
        for (int i = 0; i < colorCount; i++)
        {
            parameters.Colors[i] = ColorF.FromHSV(
                (baseHue + random.NextSingle(-variance, variance) % 1f),
                baseSat + random.NextSingle(-variance, variance),
                baseVal + random.NextSingle(-variance, variance)
                );
        }

        parameters.Vorticity = random.NextSingle() * 2;
        parameters.HasRings = random.NextSingle() < .333f;

        return parameters;
    }
}