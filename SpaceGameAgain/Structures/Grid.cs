using ImGuiNET;
using SpaceGame.Economy;
using SpaceGame.Planets;
using SpaceGame.Serialization;
using SpaceGame.Teams;
using SpaceGame.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;

[Serializable]
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
    
    [Serialize]
    public required Actor parent;
    [Serialize]
    public Dictionary<HexCoordinate, GridCell> cells = [];
    
    //public PowerLevel PowerLevel { get; private set; }
    public Actor Parent => parent;

    public override ref Transform Transform => ref parent.Transform;
    public override Transform InterpolatedTransform => parent.InterpolatedTransform;

    public Grid(GridPrototype prototype, ulong id) : base(prototype, id)
    {
    }

    public void AddCell(HexCoordinate location)
    {
        // CollisionRadius = Math.Max(CollisionRadius, location.ToCartesian().Length());
        cells.Add(location, new() { Tile = Prototypes.Get<TilePrototype>("ground_tile") });
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
            if (cell is null || cell.Structure != null)
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
            canvas.PushState();
            canvas.Translate(coord.ToCartesian());
            cell.Tile.RenderTile(canvas, this, cell, coord);

            if (World.CurrentInteractionContext == World.ConstructionInteractionContext)
            {
                if (cell?.Structure != null)
                {
                    canvas.DrawLine(hexagon[0], hexagon[1]);
                    canvas.DrawLine(hexagon[1], hexagon[2]);
                    canvas.DrawLine(hexagon[2], hexagon[3]);

                    GridCell? neighbor = GetCell(coord + new HexCoordinate(-1, 0));
                    if (neighbor is null || neighbor.Structure != null)
                    {
                        canvas.DrawLine(hexagon[3], hexagon[4]);
                    }

                    neighbor = GetCell(coord + new HexCoordinate(0, -1));
                    if (neighbor is null || neighbor.Structure != null)
                    {
                        canvas.DrawLine(hexagon[4], hexagon[5]);
                    }

                    neighbor = GetCell(coord + new HexCoordinate(1, -1));
                    if (neighbor is null || neighbor.Structure != null)
                    {
                        canvas.DrawLine(hexagon[5], hexagon[0]);
                    }
                }
            }
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

                if ((cartesian + Angle.ToVector(0 * MathF.Tau / 6)).LengthSquared() > radius * radius) continue;
                if ((cartesian + Angle.ToVector(1 * MathF.Tau / 6)).LengthSquared() > radius * radius) continue;
                if ((cartesian + Angle.ToVector(2 * MathF.Tau / 6)).LengthSquared() > radius * radius) continue;
                if ((cartesian + Angle.ToVector(3 * MathF.Tau / 6)).LengthSquared() > radius * radius) continue;
                if ((cartesian + Angle.ToVector(4 * MathF.Tau / 6)).LengthSquared() > radius * radius) continue;
                if ((cartesian + Angle.ToVector(5 * MathF.Tau / 6)).LengthSquared() > radius * radius) continue;

                grid.AddCell(new(q, r));
            }
        }
    }

    public void PlaceStructure(StructurePrototype prototype, HexCoordinate location, int rotation, Team team, List<HexCoordinate>? footprint = null)
    {
        var structure = prototype.CreateActor(World.NewID());
        structure.Team = team;
        structure.Grid = this;
        structure.Location = location;
        structure.Rotation = rotation;
        World.Add(structure);

        foreach (var footprintPart in prototype.Footprint)
        {
            var cellLocation = location + footprintPart.Rotated(rotation);
            GetCell(cellLocation)!.Structure = structure;
            // GetCell(cellLocation)!.Tile = new Tile(Prototypes.Get<TilePrototype>("foundation_tile"), World.NewID(), Transform.Default);
        }

        foreach (var cell in structure.GetAdjacentCells())
        {
            var neighbor = GetCell(cell)?.Structure;

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

        // UpdatePowerLevel();
    }

    public GridCell? GetCellFromPoint(DoubleVector point)
    {
        var localPos = this.Transform.WorldToLocal(point.ToVector2());
        var coord = HexCoordinate.FromCartesian(localPos);
        return GetCell(coord);
    }

    //public void UpdatePowerLevel()
    //{
    //    PowerLevel maxPowerLevel = PowerLevel.None;
    //    foreach (var cell in this.cells)
    //    {
    //        if (cell.Value.Structure.Actor is Structure s)
    //        {
    //            maxPowerLevel = (PowerLevel)Math.Max((int)maxPowerLevel, (int)s.Prototype.ProvidedPowerLevel);
    //        }
    //    }
    //    this.PowerLevel = maxPowerLevel;
    //}

    public override void Tick()
    {
        base.Tick();
    }

    internal void RemoveStructure(Structure structure)
    {
        foreach (var neighbor in structure.neighbors)
        {
            neighbor.neighbors.Remove(structure);
            neighbor.OnNeighborRemoved(structure);
        }

        foreach (var cellLoc in structure.Prototype.Footprint)
        {
            var cell = GetCell(structure.Location + cellLoc.Rotated(structure.Rotation));
            if (cell != null)
            {
                cell.Structure = null;
            }
        }

        // UpdatePowerLevel();
    }

    //public override void Serialize(BinaryWriter writer)
    //{
    //    writer.Write(ID);
    //    writer.Write(parent);

    //    writer.Write(cells.Count);
    //    foreach (var (coordinate, cell) in cells)
    //    {
    //        writer.Write(coordinate);
    //        writer.Write(cell.Structure);
    //        //writer.Write(cell.Tile.Prototype.Name);
    //    }
    //}

    public override void DebugLayout()
    {
        base.DebugLayout();
    }
}

class GridPrototype : Prototype
{
    public override Type ActorType => typeof(Grid);

    //public override Actor Deserialize(BinaryReader reader)
    //{
    //    ulong id = reader.ReadUInt64();
    //    ActorReference<Actor> parent = reader.ReadActorReference<Actor>();

    //    Dictionary<HexCoordinate, GridCell> cells = new();
    //    int cellCount = reader.ReadInt32();
    //    for (int i = 0; i < cellCount; i++)
    //    {
    //        HexCoordinate coordinate = reader.ReadHexCoordinate();
    //        ActorReference<Structure> cell = reader.ReadActorReference<Structure>();
    //        cells.Add(coordinate, new(Prototypes.Get<TilePrototype>("ground_tile")) { Structure = cell });
    //    }

    //    return new Grid(this, id, parent)
    //    {
    //        cells = cells,
    //    };

    //}
}