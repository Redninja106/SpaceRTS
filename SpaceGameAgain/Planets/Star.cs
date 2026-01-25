using SimulationFramework.Drawing.Shaders;
using SpaceGame.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets;
internal class Star : Planet
{
    StarShader starShader = new();
    CoronaShader coronaShader = new();

    public Star(PlanetPrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
    }

    public override void Render(ICanvas canvas)
    {
        coronaShader.time = Time.TotalTime;
        starShader.time = Time.TotalTime;
        coronaShader.starColor = starShader.starColor = this.Prototype.Color;
        coronaShader.radius = starShader.radius = this.Radius;

        canvas.Fill(coronaShader);
        canvas.DrawCircle(0, 0, this.Radius * 2);
        canvas.Fill(starShader);
        canvas.DrawCircle(0, 0, this.Radius);
        SphereOfInfluence.Render(canvas);
    }
}

class StarPrototype : PlanetPrototype
{
    public override Type ActorType => typeof(Star);
}

class StarShader : CanvasShader
{
    ITexture noiseTexture = NoiseTexture.Get();
    public ColorF starColor;
    public float time;
    public float radius;

    public override ColorF GetPixelColor(Vector2 position)
    {
        const int iterations = 12;
        const float initialScale = 100f;
        const float scaleDecay = .75f;
        const float baseBrightness = 1.4f;
        const float timeScale = 0.1f;

        Vector2 p = position / radius;
        float h = MathF.Sqrt(1f - .6f * p.LengthSquared());
        p /= h;

        float brightness = 0;
        float scale = initialScale;
        float scaleSum = 0;
        for (int i = 0; i < iterations; i++)
        {
            ColorF layerNoise = noiseTexture.Sample(new(i + .5f));
            layerNoise.R = .5f + .5f * layerNoise.R;

            Vector2 offset = Vector2.Zero;
            offset += 0.01f * new Vector2(timeScale * time * float.Cos(float.Tau * layerNoise.R), timeScale * time * float.Sin(float.Tau * layerNoise.R));
            ColorF colorNoise = noiseTexture.SampleUV((1f / scale) * p + offset);
            brightness += scale * colorNoise.R;
            scaleSum += scale;
            scale *= scaleDecay;
        }
        brightness /= scaleSum;

        float sd = position.Length() - radius;
        float sdClamp = ShaderIntrinsics.Clamp(-sd, 0, 1);
        float alpha = sdClamp;// ShaderIntrinsics.Pow(-sd, 3f) / radius;
        
        ColorF starCol = starColor;
        float m = starCol.R + starCol.G + starCol.B;
        starCol.R /= m;
        starCol.G /= m;
        starCol.B /= m;

        ColorF col = starCol * (baseBrightness + brightness);
        return col with { A = alpha };
    }

}

class CoronaShader : CanvasShader
{
    ITexture noiseTexture = NoiseTexture.Get(128);
    public ColorF starColor;
    public float time;
    public float radius;

    public override ColorF GetPixelColor(Vector2 position)
    {
        const float coronaScale = .75f;

        float signedDistance = position.Length() - radius;
        float dist = 1f - (signedDistance / (radius * coronaScale));
        float angle = float.Atan2(position.Y, position.X) + MathF.PI;

        float sampleAngle = angle / float.Tau;

        // uv repeat must not occur within a quad or we get artifacts
        if (ShaderIntrinsics.DDY(sampleAngle) > .5f)
        {
            sampleAngle += (sampleAngle < .5f ? 1 : 0);
        }

        ColorF result = ColorF.Black;
        float weight = 1;
        float sum = 0;
        for (int i = 0; i < 5; i++)
        {
            result += weight * CoronaLayer(sampleAngle / weight, dist, i / 6f);
            sum += weight;
            weight *= .666f;
        }
        result /= sum;

        //result *= float.Clamp(signedDistance, 0, .5f);
        return result;
    }

    ColorF CoronaLayer(float a, float d, float offset)
    {
        // time animation
        //sampleAngle += .5f * (adistanceNoise.R + adistanceNoise.G + adistanceNoise.B + adistanceNoise.A) * .25f;

        ColorF layerNoise = noiseTexture.SampleUV(new(offset));
        a += (layerNoise.R * 2 - 1) * time * .02f;

        ColorF angleNoise = noiseTexture.SampleUV(new(a, offset));
        // curvature based on distance
        a += .075f * d * (angleNoise.R * 2 - 1);

        ColorF noise = noiseTexture.SampleUV(new(a, offset));

        d -= noise.A * .5f;

        ColorF result = (d) * starColor * (.9f + .1f * noise.R + 0.25f * noise.G);// * (.9f + .1f * noise.R) * d);
        return result;
    }
}