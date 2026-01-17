namespace SpaceGame.Rendering;

static class NormalMapHelper
{
    public static Vector3 ExtractNormal(ColorF color)
    {
        return new Vector3(color.R * 2 - 1, color.G * 2 - 1, color.B * 2 - 1).Normalized();
    }

    public static float CalcBrightness(Vector3 normal, Vector3 lightDirection)
    {
        const float ambientLight = .1f;
        float brightness = float.Clamp(Vector3.Dot(normal, -lightDirection), 0, 1);
        return ambientLight + (1 - ambientLight) * brightness;
    }
}