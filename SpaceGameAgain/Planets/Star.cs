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
        coronaShader.time = Time.TotalTime ;
        starShader.time = Time.TotalTime * .5f;
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
        const float initialScale = 10f;
        const float scaleDecay = .75f;
        const float baseBrightness = .667f;

        float brightness = 0;
        float scale = initialScale;
        float scaleSum = 0;
        for (int i = 0; i < iterations; i++)
        {
            ColorF layerNoise = noiseTexture.Sample(new(i + .5f));
            layerNoise.R = .5f + .5f * layerNoise.R;

            Vector2 offset = Vector2.Zero;
            offset += new Vector2(time * float.Cos(float.Tau * layerNoise.R), time * float.Sin(float.Tau * layerNoise.R));
            ColorF colorNoise = noiseTexture.Sample((1f / scale) * position + offset);
            brightness += scale * colorNoise.R;
            scaleSum += scale;
            scale *= scaleDecay;
        }
        brightness /= scaleSum;

        float sd = position.Length() - radius;

        float alpha = ShaderIntrinsics.Pow(-sd, 1.4f) / radius;

        ColorF col = starColor * (baseBrightness + brightness);

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
        const float coronaScale = .3f;

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
            result += weight * CoronaLayer(sampleAngle / weight, dist* weight, i / 6f);
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
        a += .02f * d * (angleNoise.R * 2 - 1);

        ColorF noise = noiseTexture.SampleUV(new(a, offset));

        d -= noise.A * .75f;

        ColorF result = d * starColor * (.9f + .1f * noise.R);// * (.9f + .1f * noise.R) * d);
        return result;
    }
}