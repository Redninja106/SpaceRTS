using SimulationFramework.Drawing.Shaders;
//using SpaceGame.Stations;
class CompositingShader : CanvasShader
{
    public ITexture foregroundTexture;
    public ITexture visibiltyTexture;

    public CompositingShader()
    {
        noise = new int[1024];
        for (int i = 0; i < noise.Length; i++)
        {
            noise[i] = Random.Shared.Next();
        }
    }

    private int[] noise;
    public float Time = 0;
    public float Brightness = .45f;
    public float Noisiness = .15f;
    public bool renderNoise = false;

    public override ColorF GetPixelColor(Vector2 position)
    {
        float a = visibiltyTexture.Sample(position).A;
        ColorF fg = foregroundTexture.Sample(position) * a;
        ColorF noise = renderNoise ? new ColorF(0, 0, 0, CalcNoise(position) * (1 - a)) : ColorF.Transparent;
        return fg + noise;
    }

    private float CalcNoise(Vector2 position)
    {
        int seed = (int)(position.X * 17) ^ (int)(position.Y * 14);
        int offset = noise[(int)(seed + Time * 25) % noise.Length];
        float b = noise[offset % noise.Length] / (float)int.MaxValue;
        return 1 - (Brightness + (b * Noisiness));
    }

    public void Composite(ICanvas targetCanvas, ITexture texture, ITexture visibilityTexture, bool renderNoise)
    {
        this.foregroundTexture = texture;

        targetCanvas.PushState();
        targetCanvas.ResetState();
        this.foregroundTexture = texture;
        this.visibiltyTexture = visibilityTexture;
        this.Time = SimulationFramework.Time.TotalTime;
        this.renderNoise = renderNoise;
        targetCanvas.Fill(this);
        targetCanvas.DrawRect(0, 0, targetCanvas.Width, targetCanvas.Height);
        targetCanvas.PopState();
    }
}