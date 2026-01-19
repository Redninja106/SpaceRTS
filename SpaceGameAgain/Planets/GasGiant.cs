using SimulationFramework.Drawing.Shaders;
using SimulationFramework.Drawing.Shaders.Compiler;
using SpaceGame.Rendering;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets;
internal class GasGiant : Planet
{
    public override GasGiantPrototype Prototype => (GasGiantPrototype)base.Prototype;
    
    GasGiantShader shader = new(); 

    public GasGiant(PlanetPrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
    }

    public override void Render(ICanvas canvas)
    {
        shader.Setup(this);
        canvas.Fill(shader);
        canvas.DrawCircle(0, 0, Radius);

        SphereOfInfluence.Render(canvas);
    }
}

class GasGiantPrototype : PlanetPrototype
{
    public override Type ActorType => typeof(GasGiant);

    public ColorF[] Colors { get; set; } = [];
    public float Vorticity { get; set; } = 1;
    
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
        float weakH = MathF.Sqrt(1f - .7f * dir.LengthSquared());
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
        float s = MathF.Sin(.5f * (p.Y + 1) * MathF.PI * colors.Length);
        p.X += .15f * s * s * vorticity;
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
        float ti = ShaderIntrinsics.Mod(time * flowSpeed, 4f);
        float t = ShaderIntrinsics.Fract(time * flowSpeed);
        if (ti < 1) return ShaderIntrinsics.Lerp(sample1.R, sample1.G, t);
        if (ti < 2) return ShaderIntrinsics.Lerp(sample1.G, sample1.B, t);
        if (ti < 3) return ShaderIntrinsics.Lerp(sample1.B, sample1.A, t);
        else return ShaderIntrinsics.Lerp(sample1.A, sample1.R, t);
        // return ShaderIntrinsics.Lerp(sample1.R, sample1.G, ShaderIntrinsics.Fract(time));
    }

    private ColorF BaseGradient(Vector2 p)
    {
        float t = ShaderIntrinsics.Fract((MathF.Sin(p.X * 10f + time) * 0.01f + p.Y) * .5f + .5f);
        int index = (int)(t * colors.Length);
        ColorF from = colors[index];
        ColorF to = colors[(index + 1) % colors.Length];
        ColorF gradientColor = ColorF.Lerp(from, to, SmoothStep(0, 1, ShaderIntrinsics.Fract(t * colors.Length)));

        float noiseColor = noise.SampleUV(p).G;
        float brightness = 1 - (MathF.Pow(noiseColor, 4f) * vorticity);

        return gradientColor * brightness;
    }

    public static float SmoothStep(float a, float b, float t)
    {
        float num = ShaderIntrinsics.Clamp((t - a) / (b - a), 0f, 1f);
        return num * num * (3f - 2f * num);
    }

    public void Setup(GasGiant planet)
    {
        noise.Filter = TextureFilter.Linear;
        radius = planet.Radius;
        colors = planet.Prototype.Colors;
        vorticity = planet.Prototype.Vorticity;
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
