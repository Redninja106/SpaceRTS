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
    public Model Model { get; }
    public HexCoordinate[] Footprint { get; }
    public Vector2[] Outline { get; }
    public int Price { get; }
    public string? PresetModel { get; }

    public StructurePrototype(string title, int price, Model model, string? presetModel, HexCoordinate[] footprint)
    {
        this.Title = title;
        this.Price = price;
        this.PresetModel = presetModel;
        this.Model = model ?? PresetModels.presetModels[presetModel!];
        this.Footprint = footprint;

        this.Outline = CreateOutline(footprint);
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

    
    public virtual Structure CreateStructure(ulong id, Grid grid, HexCoordinate location, int rotation, Team team)
    {
        return new Structure(this, id, grid, location, rotation, team.AsReference());
    }

    public override Actor? Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();

        ActorReference<Team> team = reader.ReadActorReference<Team>();
        ActorReference<Actor> gridParent = reader.ReadActorReference<Actor>();

        int q = reader.ReadInt32();
        int r = reader.ReadInt32();
        int rotation = reader.ReadInt32();

        return new Structure(this, id, ((Planet)gridParent.Actor!).Grid, new(q, r), rotation, team);

    }
}
