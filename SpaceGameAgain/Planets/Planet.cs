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

[Serializable]
internal class Planet : Actor
{
    public override PlanetPrototype Prototype => (PlanetPrototype)base.Prototype;

    [field: Serialize]
    public float Radius { get; set; } = 26;

    //public Color Color 
    //{ 
    //    get
    //    {
    //        return shader.color.ToColor();
    //    }
    //    init
    //    {
    //        shader.color = value.ToColorF();
    //    }
    //}

    public Grid Grid => grid;

    [field: Serialize]
    public SphereOfInfluence SphereOfInfluence { get; set; }

    [Serialize]
    private Grid grid;
    [Serialize]
    public required Orbit? orbit;
    private PlanetShader shader;

    [DebugOverlay]
    public static bool ShowOrbits;

    public Planet(PlanetPrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
        shader = new PlanetShader();
    }

    public override void InitializeActor()
    {
        base.InitializeActor();

        if (grid == null)
        {
            this.grid = new Grid(Prototypes.Get<GridPrototype>("grid"), World, World.NewID()) { parent = this };
            World.Add(this.grid);
        }

        if (this.orbit != null)
        {
            this.Teleport(this.orbit.GetLocation());
        }

        SphereOfInfluence = new() { planet = this };
        SphereOfInfluence.Initialize();
    }

    public override void FinishDeserialization()
    {
        base.FinishDeserialization();
        SphereOfInfluence.Initialize();
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

        if (ShowOrbits && this.orbit != null)
        {
            DebugDraw.Circle(Vector2.Zero, this.orbit.radius, this.orbit.center.Transform);
        }
    }

    private void DrawPlanetBreakdown(ICanvas canvas)
    {
        canvas.DrawAlignedText("planet", .04f, 0, -.1f, Alignment.BottomCenter);
        canvas.Fill(Color.Yellow);
        // canvas.DrawAlignedText("power: " + NetPower, .03f, 0, -.05f, Alignment.BottomCenter);
    }

    public void TickOrbit()
    {
        if (orbit != null)
        {
            orbit.Tick();
            this.Transform = orbit.GetLocation();
        }
    }

    //public override void Serialize(BinaryWriter writer)
    //{
    //    writer.Write(ID);
    //    writer.Write(Transform);
    //    writer.Write(Radius);
    //    writer.Write(Color.Value);
    //    writer.Write(SphereOfInfluence.Radius);

    //    writer.Write(grid);

    //    writer.Write(Orbit is not null);

    //    if (Orbit is not null)
    //    {
    //        writer.Write(Orbit.center);
    //        writer.Write(Orbit.phase);
    //        writer.Write(Orbit.radius);
    //    }

    //}

    public override void DebugLayout()
    {
        base.DebugLayout();

        if (ImGui.CollapsingHeader("Planet"))
        {
        }
    }
}
