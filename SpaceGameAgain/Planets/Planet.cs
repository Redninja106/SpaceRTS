using ImGuiNET;
using SimulationFramework.Drawing.Shaders.Compiler;
using SpaceGame.Economy;
using SpaceGame.Rendering;
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
    public override PlanetPrototype Prototype => (PlanetPrototype)base.Prototype;

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
            this.orbit = value;
        }
    }

    public Grid Grid => grid.Actor!;

    public SphereOfInfluence SphereOfInfluence { get; }

    private ActorReference<Grid> grid;
    private Orbit? orbit;
    private PlanetShader shader;

    public Planet(PlanetPrototype prototype, ulong id, Transform transform, Orbit? orbit, ActorReference<Grid> grid = default) : base(prototype, id, transform)
    {
        this.orbit = orbit;

        if (this.orbit != null)
        {
            this.Teleport(this.orbit.GetLocation());
        }

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
        shader.rad = this.Radius; 
        Vector2 v = (this.Transform.Position).ToVector2().Normalized();
        shader.lightDir = new Vector3(v.X, -v.Y, -1).Normalized();
        shader.time = Time.TotalTime;
        shader.texture = Prototype.Material.Texture;
        shader.texScale =  (128 * float.Sqrt(3));
        if (Prototype.Material.NormalMap != null)
        {
            shader.normalMap = Prototype.Material.NormalMap;
            shader.normalMapEffect = 1;
        }
        canvas.Fill(shader);
        canvas.DrawCircle(0, 0, Radius);

        if (World.Camera.SmoothVerticalSize < Radius * 10 && World.GetSphereOfInfluence(World.MousePosition) == this.SphereOfInfluence)
        {
            canvas.PushState();
            canvas.Translate(0, -Radius);
            canvas.Scale(World.Camera.SmoothVerticalSize);
            //DrawPlanetBreakdown(canvas);
            canvas.PopState();
        }

        SphereOfInfluence.Render(canvas);
    }

    private void DrawPlanetBreakdown(ICanvas canvas)
    {
        canvas.DrawAlignedText("planet", .04f, 0, -.1f, Alignment.BottomCenter);
        canvas.Fill(Color.Yellow);
        // canvas.DrawAlignedText("power: " + NetPower, .03f, 0, -.05f, Alignment.BottomCenter);
    }

    public void TickOrbit()
    {
        if (Orbit != null)
        {
            Orbit.Tick(Program.Timestep);
            this.Transform = Orbit.GetLocation();
        }
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);
        writer.Write(Transform);
        writer.Write(Radius);
        writer.Write(Color.Value);
        writer.Write(SphereOfInfluence.Radius);

        writer.Write(grid);

        writer.Write(Orbit is not null);

        if (Orbit is not null)
        {
            writer.Write(Orbit.center);
            writer.Write(Orbit.phase);
            writer.Write(Orbit.radius);
        }

    }

    public override void DebugLayout()
    {
        base.DebugLayout();

        if (ImGui.CollapsingHeader("Planet"))
        {
        }
    }
}
