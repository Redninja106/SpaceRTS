using SimulationFramework.Drawing.Shaders;
//using SpaceGame.Stations;
class VisibilityCompositingShader : CanvasShader
{
    public ITexture foregroundTexture;
    public ITexture visibiltyTexture;

    public override ColorF GetPixelColor(Vector2 position)
    {
        return foregroundTexture.Sample(position) * visibiltyTexture.Sample(position).A;
    }

    public void Composite(ICanvas targetCanvas, ITexture texture, ITexture visibilityTexture, bool renderNoise)
    {
        this.foregroundTexture = texture;

        targetCanvas.PushState();
        targetCanvas.ResetState();
        this.foregroundTexture = texture;
        this.visibiltyTexture = visibilityTexture;
        targetCanvas.Fill(this);
        targetCanvas.DrawRect(0, 0, targetCanvas.Width, targetCanvas.Height);
        targetCanvas.PopState();
    }
}