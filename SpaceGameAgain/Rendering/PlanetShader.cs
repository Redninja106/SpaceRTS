using SimulationFramework.Drawing.Shaders;
using SpaceGame.Structures;

namespace SpaceGame.Rendering;

class PlanetShader : CanvasShader
{
    public float time;
    public ColorF color;
    public float rad;
    public Vector3 lightDir;
    public ITexture texture;
    public ITexture normalMap;
    public float normalMapEffect = 0;
    public float texScale = 1;
    public float ambientLight = .5f;

    public override ColorF GetPixelColor(Vector2 position)
    {
        Vector2 dir = position / rad;
        float h = MathF.Sqrt(1f - dir.LengthSquared());

        // Vector3 normal = new(dir.X, h , dir.Y);

        // float brightness = MathF.Min(MathF.Max(-Vector3.Dot(normal.Normalized(), lightDir.Normalized()) * 2, 0), 1);
        // brightness += .01f * (Util.ShaderNoise(new Vector2(position.X * time, position.Y * time)) * 2 - 1);
        // brightness = .5f + .5f * brightness;
        //return this.color * brightness;

        HexCoordinate hexCoord = HexCoordinate.FromCartesian(position);

        float jitter = Util.ShaderNoise(new(hexCoord.Q, hexCoord.R));

        Vector2 texPos = position * 1f * texScale + 10000f * new Vector2(Util.ShaderNoise(hexCoord.R, hexCoord.S), Util.ShaderNoise(hexCoord.S, hexCoord.Q));
        Vector3 normal = NormalMapHelper.ExtractNormal(normalMap.Sample(texPos));
        float brightness = NormalMapHelper.CalcBrightness(normal, lightDir);

        // float brightness = float.Clamp(Vector3.Dot(normalMapNormal.Normalized(), -lightDir), 0, 1);

        ColorF color = texture.Sample(texPos);


        // color.R += jitter * 0.02f;
        // color.G += jitter * 0.02f;

        // color.B += jitter * 0.02f;
        if (normalMapEffect > 0)
        {
            color *= new ColorF(brightness, brightness, brightness, 1);
        }
        color.A = 1;
        return color;
    }

}
