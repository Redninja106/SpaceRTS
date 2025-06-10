using SimulationFramework.Drawing.Shaders;

namespace SpaceGame.Rendering;

class NoiseShader : CanvasShader
{
    public int[] noise;

    public NoiseShader()
    {
        noise = new int[1024];
        for (int i = 0; i < noise.Length; i++)
        {
            noise[i] = Random.Shared.Next();
        }
    }

    public float Time = 0;
    public float Brightness = .75f;
    public float Noisiness = .05f;

    public override ColorF GetPixelColor(Vector2 position)
    {
        // asdasd
        int seed = (int)(position.X * 17) ^ (int)(position.Y * 14);
        int offset = noise[(int)(seed + Time * 25) % noise.Length];
        float b = noise[offset % noise.Length] / (float)int.MaxValue;
        return new ColorF(0, 0, 0, 1 - (Brightness + b * Noisiness));
    }
}
