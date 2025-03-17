using SpaceGame.Economy;
using SpaceGame.Planets;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class StructurePrototype : UnitPrototype
{
    public HexCoordinate[] Footprint { get; set; } = [HexCoordinate.Zero];
    public int Price { get; set; }
    public string? PresetModel { get; set; } = "default";
    public Vector2 Center { get; set; }
    public bool CanBeRotated { get; set; } = true;
    public int PowerProduced { get; set; } = 0;
    public int PowerConsumed { get; set; } = 0;
    public Model Model { get; private set; } = null!;
    public Vector2[] Outline { get; private set; } = [];
    public Dictionary<ResourcePrototype, int> Cost { get; set; } = [];

    public StructurePrototype()
    {

    }

    public override void InitializePrototype()
    {
        base.InitializePrototype();
        this.Center = ComputeCenter(this.Footprint);
        this.Outline = CreateOutline(this.Footprint);

        this.Model ??= PresetModels.presetModels[this.PresetModel!];
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

    
    public virtual Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    {
        return new Structure(this, id, grid, location, rotation, team);
    }

    public override WorldActor Deserialize(BinaryReader reader)
    {
        DeserializeArgs(reader, out var id, out var team, out var grid, out var location, out var rotation);
        return CreateStructure(id, team, grid, location, rotation);
    }

    public void DeserializeArgs(BinaryReader reader, out ulong id, out ActorReference<Team> team, out ActorReference<Grid> grid, out HexCoordinate location, out int rotation)
    {
        id = reader.ReadUInt64();
        team = reader.ReadActorReference<Team>();
        grid = reader.ReadActorReference<Grid>();
        location = reader.ReadHexCoordinate();
        rotation = reader.ReadInt32();
    }

    [DebugButton]
    public void Build()
    {
        var ctorShip = World.SelectionHandler.GetSelectedUnits().OfType<Ship>().FirstOrDefault(u => u is Ship s && s.modules.Any(m => m!.Actor is ConstructionModule));
        if (ctorShip != null)
        {
            World.ConstructionInteractionContext.BeginPlacing(this, ctorShip);
        }
    }
}
