using SpaceGame.Planets;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class Grid : WorldActor
{
    public static Vector2[] hexagon = [
        Angle.ToVector(0 * MathF.Tau / 6),
        Angle.ToVector(1 * MathF.Tau / 6),
        Angle.ToVector(2 * MathF.Tau / 6),
        Angle.ToVector(3 * MathF.Tau / 6),
        Angle.ToVector(4 * MathF.Tau / 6),
        Angle.ToVector(5 * MathF.Tau / 6),
    ];

    public Dictionary<HexCoordinate, GridCell> cells = [];
    public List<ActorReference<Structure>> structures = [];
    private ActorReference<WorldActor> parent;

    public WorldActor Parent => parent.Actor!;

    public override ref Transform Transform => ref parent.Actor!.Transform;
    public override Transform InterpolatedTransform => parent.Actor!.InterpolatedTransform;

    public Grid(GridPrototype prototype, ulong id, ActorReference<WorldActor> parent) : base(prototype, id, Transform.Default)
    {
        this.parent = parent;
    }

    public void AddCell(HexCoordinate location)
    {
        cells.Add(location, new());
    }

    public void RemoveCell(HexCoordinate location)
    {
        cells.Remove(location);
    }

    public bool IsStructureObstructed(StructurePrototype structure, HexCoordinate location, int rotation)
    {
        foreach (var footprintCell in structure.Footprint)
        {
            var cell = GetCell(location + footprintCell.Rotated(rotation));
            if (cell is null || !cell.Structure.IsNull)
            {
                return true;
            }
        }
        return false;
    }

    public GridCell? GetCell(HexCoordinate coord)
    {
        return cells.TryGetValue(coord, out var cell) ? cell : null;
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Stroke(Color.LightGray with { A = 50 });
        foreach (var (coord, cell) in cells)
        {
            if (cell?.Structure.IsNull ?? false)
            {
                canvas.PushState();
                canvas.Translate(coord.ToCartesian());
                canvas.DrawLine(hexagon[0], hexagon[1]);
                canvas.DrawLine(hexagon[1], hexagon[2]);
                canvas.DrawLine(hexagon[2], hexagon[3]);

                GridCell? neighbor = GetCell(coord + new HexCoordinate(-1, 0));
                if (neighbor is null || !neighbor.Structure.IsNull)
                {
                    canvas.DrawLine(hexagon[3], hexagon[4]);
                }

                neighbor = GetCell(coord + new HexCoordinate(0, -1));
                if (neighbor is null || !neighbor.Structure.IsNull)
                {
                    canvas.DrawLine(hexagon[4], hexagon[5]);
                }

                neighbor = GetCell(coord + new HexCoordinate(1, -1));
                if (neighbor is null || !neighbor.Structure.IsNull)
                {
                    canvas.DrawLine(hexagon[5], hexagon[0]);
                }

                canvas.PopState();
            }
        }
    }

    public static void FillRadius(Grid grid, float radius)
    {
        for (int q = -(int)radius; q < radius; q++)
        {
            for (int r = -(int)radius; r < radius; r++)
            {
                HexCoordinate coord = new(q, r);
                Vector2 cartesian = coord.ToCartesian();

                if ((cartesian + Angle.ToVector(0 * MathF.Tau / 6)).Length() > radius) continue;
                if ((cartesian + Angle.ToVector(1 * MathF.Tau / 6)).Length() > radius) continue;
                if ((cartesian + Angle.ToVector(2 * MathF.Tau / 6)).Length() > radius) continue;
                if ((cartesian + Angle.ToVector(3 * MathF.Tau / 6)).Length() > radius) continue;
                if ((cartesian + Angle.ToVector(4 * MathF.Tau / 6)).Length() > radius) continue;
                if ((cartesian + Angle.ToVector(5 * MathF.Tau / 6)).Length() > radius) continue;

                grid.AddCell(new(q, r));
            }
        }
    }

    public void PlaceStructure(StructurePrototype prototype, HexCoordinate location, int rotation, Team team, List<HexCoordinate>? footprint = null)
    {
        var structure = prototype.CreateStructure(World.NewID(), team.AsReference(), this.AsReference(), location, rotation);
        World.Add(structure);

        foreach (var footprintPart in prototype.Footprint)
        {
            var cellLocation = location + footprintPart.Rotated(rotation);
            GetCell(cellLocation)!.Structure = structure.AsReference();
        }

        foreach (var cell in structure.GetAdjacentCells())
        {
            var neighbor = GetCell(structure.Location + cell)?.Structure.Actor;

            if (neighbor != null)
            {
                if (structure.neighbors.Add(neighbor))
                {
                    structure.OnNeighborAdded(neighbor);
                }
                if (neighbor.neighbors.Add(structure))
                {
                    neighbor.OnNeighborAdded(structure);
                }
            }
        }
    }

    public GridCell? GetCellFromPoint(Vector2 point, Transform transform)
    {
        var localPos = this.Transform.WorldToLocal(transform.LocalToWorld(point));
        var coord = HexCoordinate.FromCartesian(localPos);
        return GetCell(coord);
    }

    public override void Tick()
    {
        base.Tick();
    }


    internal void RemoveStructure(Structure structure)
    {
        foreach (var adj in structure.GetAdjacentCells())
        {
            var neighbor = GetCell(structure.Location + adj)?.Structure.Actor;
            neighbor?.OnNeighborRemoved(structure);
        }

        foreach (var cellLoc in structure.Prototype.Footprint)
        {
            var cell = GetCell(structure.Location + cellLoc.Rotated(structure.Rotation));
            if (cell != null)
            {
                cell.Structure = ActorReference<Structure>.Null;
            }
        }
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);
        writer.Write(parent);

        writer.Write(structures.Count);
        foreach (var actor in structures)
        {
            writer.Write(actor);
        }

        writer.Write(cells.Count);
        foreach (var (coordinate, cell) in cells)
        {
            writer.Write(coordinate);
            writer.Write(cell.Structure);
        }
    }
}

class GridPrototype : WorldActorPrototype
{
    public override WorldActor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        ActorReference<WorldActor> parent = reader.ReadActorReference<WorldActor>();

        List<ActorReference<Structure>> structures = new();
        int structureCount = reader.ReadInt32();
        for (int i = 0; i < structureCount; i++)
        {
            structures.Add(reader.ReadActorReference<Structure>());
        }

        Dictionary<HexCoordinate, GridCell> cells = new();
        int cellCount = reader.ReadInt32();
        for (int i = 0; i < cellCount; i++)
        {
            HexCoordinate coordinate = reader.ReadHexCoordinate();
            ActorReference<Structure> cell = reader.ReadActorReference<Structure>();
            cells.Add(coordinate, new() { Structure = cell });
        }

        return new Grid(this, id, parent)
        {
            cells = cells,
            structures = structures,
        };

    }
}