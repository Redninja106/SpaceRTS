using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class Grid
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
    private Actor parent;

    public Actor Parent => parent;

    public ref Transform Transform => ref parent.Transform;

    public Grid(Actor parent)
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

    public void Render(ICanvas canvas)
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
        var structure = prototype.CreateStructure(World.NewID(), this, location, rotation, team);
        World.Add(structure);

        foreach (var footprintPart in prototype.Footprint)
        {
            var cellLocation = location + footprintPart.Rotated(rotation);
            GetCell(cellLocation)!.Structure = structure.AsReference();
        }
    }

    public GridCell? GetCellFromPoint(Vector2 point, Transform transform)
    {
        var localPos = this.Transform.WorldToLocal(transform.LocalToWorld(point));
        var coord = HexCoordinate.FromCartesian(localPos);
        return GetCell(coord);
    }

    public void Update()
    {
        
    }


    internal void RemoveStructure(Structure instance)
    {
        foreach (var cellLoc in instance.Prototype.Footprint)
        {
            var cell = GetCell(instance.Location + cellLoc.Rotated(instance.Rotation));
            if (cell != null)
            {
                cell.Structure = ActorReference<Structure>.Null;
            }
        }
    }
}
