using SpaceGame.Economy;
using SpaceGame.Extensions;
using SpaceGame.GUI;
using SpaceGame.Planets;
using SpaceGame.Rendering;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class StructurePrototype : UnitPrototype, IGUIProvider
{
    public override Type ActorType => typeof(Structure);

    public HexCoordinate[] Footprint { get; set; } = [HexCoordinate.Zero];
    // public string PresetModel { get; set; } = "default";
    public Vector2 Center { get; set; }
    public bool CanBeRotated { get; set; } = true;
    public Vector2[] Outline { get; private set; } = [];
    public HexCoordinate[] AdjacentCells { get; private set; } = [];
    public Dictionary<ResourcePrototype, int> ResourceCosts { get; set; } = [];

    [JsonConverter(typeof(JsonStringEnumConverter<PowerLevel>))]
    public PowerLevel ProvidedPowerLevel { get; set; } = PowerLevel.None;
    [JsonConverter(typeof(JsonStringEnumConverter<PowerLevel>))]
    public PowerLevel RequiredPowerLevel { get; set; } = PowerLevel.None;

    //[JsonIgnore]
    //public ITexture Icon => Icons.Structure;

    public StructurePrototype()
    {
    }

    public override void InitializePrototype()
    {
        base.InitializePrototype();
        this.Center = ComputeCenter(this.Footprint);
        this.Outline = CreateOutline(this.Footprint);
        this.AdjacentCells = CreateAdjacentCells(this.Footprint);
        this.CollisionRadius = MathF.Sqrt(this.Outline.Max(p => (p - Center).LengthSquared()));

        if (RevealRadius < CollisionRadius + 2f)
        {
            RevealRadius = CollisionRadius + 2f;
        }

        this.CanBeRotated = this.Model.SpriteCount > 1;

        Category ??= Prototypes.Get<ConstructionCategory>("default_category");
        // this.Model ??= PresetModels.presetModels[this.PresetModel!];
    }

    private HexCoordinate[] CreateAdjacentCells(HexCoordinate[] footprint)
    {
        List<HexCoordinate> adjacents = [];
        foreach (var cell in footprint)
        {
            for (int i = 0; i < 6; i++)
            {
                HexCoordinate adjacent = cell + HexCoordinate.UnitQ.Rotated(i);
                if (!footprint.Contains(adjacent) && !adjacents.Contains(adjacent))
                {
                    adjacents.Add(adjacent);
                }
            }
        }
        return adjacents.ToArray();
    }

    private Vector2 ComputeCenter(HexCoordinate[] footprint)
    {
        return footprint.Select(h => h.ToCartesian()).Aggregate((a, b) => a + b) * (1f / footprint.Length);
    }

    public static Vector2[] CreateOutline(HexCoordinate[] footprint)
    {
        List<Vector2> segments = [];

        foreach (var coordinate in footprint)
        {
            for (int i = 0; i < 6; i++)
            {
                if (!footprint.Contains(coordinate + HexCoordinate.UnitQ.Rotated(i)))
                {
                    segments.Add(coordinate.ToCartesian() + Angle.ToVector((i + 0) * (MathF.Tau / 6f)));
                    segments.Add(coordinate.ToCartesian() + Angle.ToVector((i + 1) * (MathF.Tau / 6f)));
                }
            }
        }

        return segments.ToArray();
    }

    public override Structure CreateActor(GameWorld world, ulong id) => (Structure)base.CreateActor(world, id);

    //public virtual Structure CreateStructure(ulong id, Team team, Grid grid, HexCoordinate location, int rotation)
    //{
    //    return new Structure(this, id)
    //    {
    //        Grid = grid,
    //        Team = team,
    //        Location = location,
    //        Rotation = rotation,
    //    };
    //}

    //public override Actor Deserialize(BinaryReader reader)
    //{
    //    DeserializeArgs(reader, out var id, out var team, out var grid, out var location, out var rotation);
    //    return CreateStructure(id, team, grid, location, rotation);
    //}

    //public void DeserializeArgs(BinaryReader reader, out ulong id, out ActorReference<Team> team, out ActorReference<Grid> grid, out HexCoordinate location, out int rotation)
    //{
    //    id = reader.ReadUInt64();
    //    team = reader.ReadActorReference<Team>();
    //    grid = reader.ReadActorReference<Grid>();
    //    location = reader.ReadHexCoordinate();
    //    rotation = reader.ReadInt32();
    //}

    public override void Layout(GUIWindow window)
    {
        bool unlocked = Program.World.PlayerTeam.IsUnlocked(this); 

        using (window.Row())
        {
            if (unlocked)
            {
                window.ModelImage(this.Model, new(64, 64));
            }
            else
            {
                window.Image(Icon.Default.Texture64x64, tint: ColorF.Gray);
            }

            using (window.Column())
            {
                Color color = unlocked ? GUIWindow.DefaultTextColor : Color.Gray;
                
                window.Text(unlocked ? this.Title : "undiscovered", 24, color: color);

                if (unlocked) 
                {
                    using (window.Row())
                    {
                        window.Text("$" + this.Cost + "k");

                        if (RequiredPowerLevel != PowerLevel.None)
                        {
                            for (int i = 0; i < (int)RequiredPowerLevel; i++)
                            {
                                window.Image(Icon.Get("economic_icon").Texture16x16, inline: true);

                                if (window.LastItemHovered())
                                {
                                    window.Viewport.SetTooltip(w => w.Text($"required power level: {RequiredPowerLevel.ToString().ToLower()}"));
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(this.Description))
                    {
                        window.Text(this.Description);
                    }

                    if (ProvidedPowerLevel != PowerLevel.None)
                    {
                        window.Text("provides '" + ProvidedPowerLevel.ToString() + "' power");
                    }
                }
            }
        }
    }

    public virtual void RenderAdjacencyOverlay(ICanvas canvas, Vector2 position, StructurePrototype otherPrototype)
    {
        if (this.RequiredPowerLevel != Economy.PowerLevel.None && otherPrototype.ProvidedPowerLevel >= this.RequiredPowerLevel)
        {
            ITexture icon = Rendering.Icon.Get("economic_icon").Texture64x64;
            canvas.DrawTexture(icon, position, new Vector2(.5f, .5f), Alignment.Center);
        }
    }
}
