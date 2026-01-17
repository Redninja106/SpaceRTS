using Silk.NET.OpenGL;
using SimulationFramework.Drawing.Shaders;
using SpaceGame.Planets;
using SpaceGame.Structures;

namespace SpaceGame.Rendering;

class PlanetShader : CanvasShader
{
    public float time;
    public ColorF tint;
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
        float weakH = MathF.Sqrt(1f - .6f * dir.LengthSquared());
        Vector3 surfaceNormal = new(dir.X, dir.Y, h);

        HexCoordinate hexCoord = HexCoordinate.FromCartesian(position);

        Vector2 p = position * texScale * (1 / weakH);
        Vector2 texPos = p + 10000f * new Vector2(Util.ShaderNoise(hexCoord.R + 245, hexCoord.S + 534), Util.ShaderNoise(hexCoord.S + 1563, hexCoord.Q - 151));

        float brightness = NormalMapHelper.CalcBrightness(surfaceNormal, lightDir);
        if (normalMapEffect > 0)
        {
            Vector3 pixelNormal = NormalMapHelper.ExtractNormal(normalMap.Sample(texPos));
            brightness *= NormalMapHelper.CalcBrightness(pixelNormal, lightDir);
        }

        ColorF color = brightness * this.tint * texture.Sample(texPos);
        color.A = 1;
        return color;
    }

    internal void Setup(Planet planet)
    {
        rad = planet.Radius;
        Star? star = planet.World.GetStar(planet.InterpolatedTransform.Position);
        if (star != null)
        {
            Vector2 v = (planet.Transform.Position - star.Transform.Position).ToVector2().Normalized();
            lightDir = new Vector3(v.X, v.Y, -1).Normalized();
            tint = ColorF.Lerp(ColorF.White, star.Prototype.Color, .2f);
        }
        time = Time.TotalTime;
        texture = planet.Prototype.Material.Texture;
        texScale = (128 * float.Sqrt(3));
        if (planet.Prototype.Material.NormalMap != null)
        {
            normalMap = planet.Prototype.Material.NormalMap;
            normalMapEffect = 1;
        }
    }
}
