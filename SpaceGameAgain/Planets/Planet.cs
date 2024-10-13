using SimulationFramework.Drawing.Shaders;
using SimulationFramework.Drawing.Shaders.Compiler;
using SpaceGame;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets;
internal class Planet : Actor
{
    public float Radius { get; init; } = 26;
    public Color Color 
    { 
        get
        {
            return shader.color.ToColor();
        }
        init
        {
            shader.color = value.ToColorF();
        }
    }

    public Orbit? Orbit 
    { 
        get 
        {
            return orbit;
        }
        set
        {
            value?.Update(this, 0);
            SphereOfInfluence.lastPosition = this.Transform.Position;
            this.orbit = value;
        }
    }

    public Grid Grid { get; }

    public SphereOfInfluence SphereOfInfluence { get; }

    private Orbit? orbit;
    private PlanetShader shader;

    public Planet()
    {
        Grid = new(this);
        SphereOfInfluence = new(this);
        shader = new PlanetShader();
    }

    public override void Update()
    {
        Grid.Update();
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Fill(shader);
        canvas.DrawCircle(0, 0, Radius);
        
        // Grid.Render(canvas);
        SphereOfInfluence.Render(canvas);
    }

    public void UpdateOrbit()
    {
        Orbit?.Update(this, Time.DeltaTime);
    }
}

class PlanetShader : CanvasShader
{
    public ColorF color;

    public override ColorF GetPixelColor(Vector2 position)
    {
        HexCoordinate hexCoord = HexCoordinate.FromCartesian(position);

        float jitter = noise(new(hexCoord.Q, hexCoord.R));

        ColorF color = this.color;
        color.R += jitter * 0.02f;
        color.G += jitter * 0.02f;
        color.B += jitter * 0.02f;
        return color;
    }

    float noise(Vector2 uv)
    {
        Vector2 noise = new(ShaderIntrinsics.Fract(ShaderIntrinsics.Sin(Vector2.Dot(uv, new Vector2(12.9898f, 78.233f) * 2.0f)) * 43758.5453f));
        return MathF.Abs(noise.X + noise.Y) * 0.5f;
    }
}