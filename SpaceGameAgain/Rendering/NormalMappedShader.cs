using SimulationFramework.Drawing.Shaders;
using SpaceGame.Rendering;

namespace SpaceGame;

class NormalMappedShader : CanvasShader
{
    public ITexture texture;
    public ITexture normalMap;
    public ColorF tint;
    public Vector3 lightDirection;
    public Vector2 size;

    public override ColorF GetPixelColor(Vector2 position)
    {
        Vector2 uv = (position / size) + new Vector2(.5f, .5f);
        ColorF color = texture.SampleUV(uv);

        if (color.A == 0)
        {
            ShaderIntrinsics.Discard();
        }

        Vector3 normal = NormalMapHelper.ExtractNormal(normalMap.SampleUV(uv));
        float brightness = NormalMapHelper.CalcBrightness(normal, lightDirection);

        // float a = color.A;
        color *= new ColorF(brightness, brightness, brightness, 1);
        color *= tint;
        
        return color;
    }
}