using SpaceGame.Planets;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class StructurePrototype : UnitPrototype
{
    public string Title { get; }
    public Model Model { get; } = null!;
    public HexCoordinate[] Footprint { get; }
    public Vector2[] Outline { get; }
    public int Price { get; }
    public string? PresetModel { get; }
    public HexCoordinate Center { get; set; } = HexCoordinate.Zero;
    public bool CanBeRotated { get; set; } = true;
    public int PowerProduced { get; set; } = 0;
    public int PowerConsumed { get; set; } = 0;

    public StructurePrototype(string title, int price, Model model, string? presetModel, HexCoordinate[] footprint)
    {
        this.Title = title;
        this.Price = price;
        this.PresetModel = presetModel ?? "default";
        this.Model = model ?? PresetModels.presetModels[presetModel!];
        this.Footprint = footprint ?? [HexCoordinate.Zero];

        this.Outline = CreateOutline(this.Footprint);
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
}
