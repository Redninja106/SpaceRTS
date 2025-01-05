using ImGuiNET;
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
internal class Planet : WorldActor
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

    public Grid Grid => grid.Actor!;

    public SphereOfInfluence SphereOfInfluence { get; }

    private ActorReference<Grid> grid;
    private Orbit? orbit;
    private PlanetShader shader;

    public Planet(PlanetPrototype prototype, ulong id, Transform transform, ActorReference<Grid> grid = default) : base(prototype, id, transform)
    {
        if (grid.IsNull)
        {
            this.grid = new Grid(Prototypes.Get<GridPrototype>("grid"), World.NewID(), this.AsReference().Cast<WorldActor>()).AsReference();
            World.Add(this.grid.Actor!);
        }
        else
        {
            this.grid = grid;
        }

        SphereOfInfluence = new(this);
        shader = new PlanetShader();
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Fill(shader);
        canvas.DrawCircle(0, 0, Radius);
        
        SphereOfInfluence.Render(canvas);
    }

    public void TickOrbit()
    {
        Orbit?.Update(this, Program.Timestep);
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);
        writer.Write(Transform);
        writer.Write(Radius);
        writer.Write(Color.Value);

        writer.Write(grid);

        writer.Write(Orbit is not null);

        if (Orbit is not null)
        {
            writer.Write(Orbit.center);
            writer.Write(Orbit.phase);
            writer.Write(Orbit.radius);
        }

    }

    public override void Layout()
    {
        base.Layout();

        if (ImGui.CollapsingHeader("Planet"))
        {
            ImGui.Text(Grid.Parent.ToString());
        }
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