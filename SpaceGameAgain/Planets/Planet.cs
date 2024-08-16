using SimulationFramework.Drawing.Shaders;
using SimulationFramework.Drawing.Shaders.Compiler;
using SpaceGame;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
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
        ShaderCompiler.DumpShaders = true;
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

        float jitter = MathF.Sin(1235.2634f * (hexCoord.Q * (hexCoord.R << 4) * hexCoord.S));
        
        ColorF color = this.color;
        color.R += jitter * 0.02f;
        color.G += jitter * 0.02f;
        color.B += jitter * 0.02f;
        return color;
    }
}