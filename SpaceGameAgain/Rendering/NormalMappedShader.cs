using SimulationFramework.Drawing.Shaders;

namespace SpaceGame.Rendering;

class NormalMappedShader : CanvasShader
{
    public ITexture texture;
    public ITexture shadowMap;
    public ITexture normalMap;
    public ColorF tint;
    public Vector3 lightDirection;
    public Vector2 size;
    public float shadowIndex;

    public override ColorF GetPixelColor(Vector2 position)
    {
        Vector2 uv = position / size + new Vector2(.5f, .5f);
        ColorF color = texture.SampleUV(uv);

        if (color.A == 0)
        {
            ColorF shadow = shadowMap.SampleUV(uv);

            int shadowIdxA = (int)float.Floor(shadowIndex);
            int shadowIdxB = (int)float.Ceiling(shadowIndex);

            ColorF colorA = IsInShadow(shadow, shadowIdxA) ? new(0, 0, 0, .75f) : new(0, 0, 0, 0);
            ColorF colorB = IsInShadow(shadow, shadowIdxB) ? new(0, 0, 0, .75f) : new(0, 0, 0, 0);
            
            ColorF interpolated = ColorF.Lerp(colorA, colorB, ShaderIntrinsics.Fract(shadowIndex));
            
            if (interpolated.A == 0)
            {
                ShaderIntrinsics.Discard();
            }

            return interpolated;
        }

        Vector3 normal = NormalMapHelper.ExtractNormal(normalMap.SampleUV(uv));
        float brightness = NormalMapHelper.CalcBrightness(normal, lightDirection);

        // float a = color.A;
        color *= new ColorF(brightness, brightness, brightness, 1);
        color *= tint;

        return color;
    }

    private bool IsInShadow(ColorF shadowValue, int bitIndex)
    {
        int mask = GetShadowMask(shadowValue);
        return ((mask >> bitIndex) & 1) == 0;
    }

    private int GetShadowMask(ColorF shadowValue)
    {
        int r = (int)(shadowValue.R * 255f);
        int g = (int)(shadowValue.G * 255f);
        int b = (int)(shadowValue.B * 255f);
        int a = (int)(shadowValue.A * 255f);
        return  (a << 24) | (b << 16) | (g << 8) | r;
    }
}