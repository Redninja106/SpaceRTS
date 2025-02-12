using SimulationFramework.Drawing.Shaders;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class StarShader : CanvasShader
{
    float size;
    ImmutableArray<float> noise = Enumerable.Range(0, 255).Select(r => Random.Shared.NextSingle()).ToImmutableArray();

    public override ColorF GetPixelColor(Vector2 position)
    {
        const float freq = 5f;
        Vector2 cellLoc = ShaderIntrinsics.Mod(position, freq);
        int cellX = (int)ShaderIntrinsics.Floor(position.X / freq);
        int cellY = (int)ShaderIntrinsics.Floor(position.Y / freq);

        return ColorF.Black;

        float b = NoiseSample2(cellX, cellY);
        return new(NoiseSample(cellX * cellY), NoiseSample(cellY / (float)cellX), 0);

        const float d = .25f;
        const float fadeBegin = 10;
        const float fadeEnd = 100;
        float a = ShaderIntrinsics.Clamp((fadeEnd - size) / (fadeEnd - fadeBegin), 0, 1);
        a = a * a * a;
        if (Vector2.DistanceSquared(cellLoc, new Vector2(freq*.5f) + new Vector2(Util.ShaderNoise(cellX * 12.345f, 0), Util.ShaderNoise(cellY * 12.345f, 0))) <= d * d)
        {
            return ColorF.White with { A = a };
        }

        return ColorF.Black;
        return new ColorF(position.X,position.Y,1);
    }
    private float NoiseSample2(float x, float y)
    {
        return ShaderIntrinsics.Lerp(OneNoiseSample(x * 14.523435f), OneNoiseSample(y * 79.5254f), OneNoiseSample(x * y * 51.235f));
    }

    private float NoiseSample(float n)
    {
        return ShaderIntrinsics.Lerp(OneNoiseSample(n * 14.523435f), OneNoiseSample(n * 79.5254f), OneNoiseSample(n * 51.235f));
    }

    private float OneNoiseSample(float n)
    {
        return this.noise[(int)(MathF.Abs(n) * 76.54321f) % this.noise.Length];
    }

    public void Render(ICanvas canvas, Camera camera)
    {
        canvas.PushState();
        canvas.ResetState();
        this.size = camera.SmoothVerticalSize;
        this.TransformMatrix = camera.CreateRelativeMatrix(Transform.Default);
        canvas.Fill(this);
        canvas.DrawRect(0, 0, canvas.Width, canvas.Height);
        canvas.PopState();
    }
}
