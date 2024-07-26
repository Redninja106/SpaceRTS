using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class Grid : Actor
{
    public static Vector2[] hexagon = [
        Angle.ToVector(0 * MathF.Tau / 6),
        Angle.ToVector(1 * MathF.Tau / 6),
        Angle.ToVector(2 * MathF.Tau / 6),
        Angle.ToVector(3 * MathF.Tau / 6),
        Angle.ToVector(4 * MathF.Tau / 6),
        Angle.ToVector(5 * MathF.Tau / 6),
    ];

    private Dictionary<HexCoordinate, GridCell> cells = [];
    private List<StructureInstance> structures = [];
    private Actor parent;

    public IEnumerable<StructureInstance> Structures => structures;

    public override ref Transform Transform => ref parent.Transform;

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

    public bool IsStructureObstructed(Structure structure, HexCoordinate location, int rotation)
    {
        foreach (var footprintCell in structure.Footprint)
        {
            var cell = GetCell(location + footprintCell.Rotated(rotation));
            if (cell is null || cell.Structure is not null)
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
            if (cell?.Structure is null)
            {
                canvas.PushState();
                canvas.Translate(coord.ToCartesian());
                canvas.DrawLine(hexagon[0], hexagon[1]);
                canvas.DrawLine(hexagon[1], hexagon[2]);
                canvas.DrawLine(hexagon[2], hexagon[3]);

                GridCell? neighbor = GetCell(coord + new HexCoordinate(-1, 0));
                if (neighbor is null || neighbor.Structure is not null)
                {
                    canvas.DrawLine(hexagon[3], hexagon[4]);
                }

                neighbor = GetCell(coord + new HexCoordinate(0, -1));
                if (neighbor is null || neighbor.Structure is not null)
                {
                    canvas.DrawLine(hexagon[4], hexagon[5]);
                }

                neighbor = GetCell(coord + new HexCoordinate(1, -1));
                if (neighbor is null || neighbor.Structure is not null)
                {
                    canvas.DrawLine(hexagon[5], hexagon[0]);
                }

                canvas.PopState();
            }
        }

        foreach (var structure in structures)
        {
            canvas.PushState();
            canvas.Translate(structure.Location.ToCartesian());
            canvas.Rotate(structure.Rotation * (MathF.Tau / 6f));
            structure.RenderShadow(canvas, Vector2.TransformNormal(structure.Transform.Position.Normalized() * .4f, Matrix3x2.CreateRotation(-structure.Rotation * (MathF.Tau / 6f))));
            canvas.PopState();
        }

        foreach (var structure in structures)
        {
            canvas.PushState();
            canvas.Translate(structure.Location.ToCartesian());
            canvas.Rotate(structure.Rotation * (MathF.Tau / 6f));
            structure.Render(canvas);
            canvas.PopState();
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

    public void PlaceStructure(Structure structure, HexCoordinate location, int rotation, Team team)
    {
        var instance = structure.CreateInstance(this, location, rotation, team);
        structures.Add(instance);

        foreach (var footprintPart in structure.Footprint)
        {
            var cellLocation = location + footprintPart.Rotated(rotation);
            GetCell(cellLocation)!.Structure = instance;
        }
    }

    public GridCell? GetCellFromPoint(Vector2 point, Transform transform)
    {
        var localPos = this.Transform.WorldToLocal(transform.LocalToWorld(point));
        var coord = HexCoordinate.FromCartesian(localPos);
        return GetCell(coord);
    }

    public override void Update(float tickProgress)
    {
        for (int i = 0; i < structures.Count; i++)
        {
            structures[i].Update(tickProgress);

            if (structures[i].IsDestroyed)
            {
                RemoveStructureAt(i);
                i--;
            }
        }
    }

    private void RemoveStructureAt(int index)
    {
        var instance = structures[index];
        structures.RemoveAt(index);

        foreach (var cellLoc in instance.Structure.Footprint)
        {
            var cell = GetCell(instance.Location + cellLoc.Rotated(instance.Rotation));
            if (cell != null)
            {
                cell.Structure = null;
            }
        }
    }
}
