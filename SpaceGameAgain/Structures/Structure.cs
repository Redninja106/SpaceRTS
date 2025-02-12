using SpaceGame.Interaction;
using SpaceGame.Planets;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Structures.Zones;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class Structure : Unit
{
    public override StructurePrototype Prototype => (StructurePrototype)base.Prototype;

    public HexCoordinate Location { get; set; }
    public int Rotation { get; set; }
    public Grid Grid => grid.Actor!;
    public List<HexCoordinate>? Footprint { get; set; }

    private Vector2[]? outline;
    private ActorReference<Grid> grid;

    public HashSet<Structure> neighbors = [];

    public bool Enabled { get; set; }

    public Structure(StructurePrototype prototype, ulong id, ActorReference<Grid> grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid.Actor!.Transform.Translated(DoubleVector.FromVector2(location.ToCartesian())).Rotated(rotation * (MathF.Tau / 6f)), team)
    {
        Location = location;
        Rotation = rotation;
        this.grid = grid;

        var planet = (Planet)Grid.Parent;
        planet.PowerProduced += Prototype.PowerProduced;
        planet.PowerConsumed += Prototype.PowerConsumed;
    }

    //public override ref Transform Transform 
    //{
    //    get 
    //    { 
    //        base.Transform = Grid.Transform.Translated(DoubleVector.FromVector2(Location.ToCartesian())).Rotated(Rotation * (MathF.Tau / 6f));
    //        return ref base.Transform;
    //    }
    //}
    //public override Transform InterpolatedTransform => base.InterpolatedTransform;

    //public Structure(HexCoordinate location, int rotation, Grid grid, StructurePrototype structure, Team team, Type? behaviorType, List<HexCoordinate>? footprint)
    //{
    //    Location = location;
    //    Rotation = rotation;
    //    Structure = structure;
    //    Grid = grid;
    //    this.Team = team;
    //    this.Footprint = footprint;
    //    ComputeOutline();

    //    if (behaviorType != null)
    //        Behavior = (StructureBehavior)Activator.CreateInstance(behaviorType, this)!;
    //}

    public void ComputeOutline()
    {
        if (Footprint != null)
        {
            outline = StructurePrototype.CreateOutline(Footprint.ToArray());
        }
    }

    public IEnumerable<HexCoordinate> GetAdjacentCells()
    {
        foreach (var cell in Prototype.Footprint)
        {
            for (int i = 0; i < 6; i++)
            {
                HexCoordinate neighbor = cell + HexCoordinate.UnitQ.Rotated(i);
                if (Prototype.Footprint.Contains(neighbor))
                {
                    continue;
                }
                if (Grid.GetCell(neighbor) is null)
                {
                    continue;
                }

                yield return neighbor;
            }
        }
    }

    public override bool TestPoint(Vector2 point, Transform transform)
    {
        Vector2 localPoint = Grid.Transform.WorldToLocal(transform.LocalToWorld(point));
        HexCoordinate coord = HexCoordinate.FromCartesian(localPoint);
        return Grid.GetCell(coord) is GridCell cell && cell.Structure.Actor == this;
    }

    public override void Render(ICanvas canvas)
    {
        bool isSelected = World.SelectionHandler.IsSelected(this);
        if (isSelected)
        {
            var outline = this.outline ?? Prototype.Outline;

            for (int i = 0; i < outline.Length; i += 2)
            {
                canvas.Stroke(Team.Actor?.GetRelationColor(World.PlayerTeam.Actor!) ?? Teams.Team.NeutralColor);
                canvas.StrokeWidth(0);
                canvas.DrawLine(outline[i], outline[i + 1]);
            }
        }

        if (((Planet)Grid.Parent).NetPower < 0 && Prototype.PowerConsumed > 0)
        {
            Prototype.Model.Render(canvas);
            Prototype.Model.Render(canvas, new RenderParameters() { colorOverride = Color.Black with { A = 100 } });
            canvas.Fill(Color.Red);
            canvas.DrawAlignedText("no power", .25f, Prototype.Center, Alignment.Center);
            return;
        }

        if (Prototype is ZonedStructurePrototype zone)
        {
            if (!isSelected && ((World.SelectionHandler.GetSelectedObject() as Ship)?.modules?.Any(m => m is ConstructionModule) ?? false))
            {
                canvas.Fill((zone.Color with { A = .5f }));
                foreach (var cell in Prototype.Footprint)
                {
                    canvas.PushState();
                    canvas.Translate(cell.ToCartesian());
                    canvas.DrawPolygon(Grid.hexagon);
                    canvas.PopState();
                }
            }

            //if (Behavior != null)
            //{
            //    Behavior?.RenderBeforeCells(canvas);

            //    foreach (var cell in Footprint ?? Prototype.Footprint)
            //    {
            //        canvas.PushState();
            //        canvas.Translate(cell.ToCartesian());
            //        Behavior?.RenderCell(canvas, cell);
            //        canvas.PopState();
            //    }

            //    Behavior?.RenderAfterCells(canvas);
            //}
        }
        else
        {
            Prototype.Model.Render(canvas);
        }
    }

    public void RenderShadow(ICanvas canvas, Vector2 offset)
    {
        if (Prototype is ZonedStructurePrototype zone)
        {
            foreach (var cell in Prototype.Footprint)
            {
                canvas.PushState();
                canvas.Translate(cell.ToCartesian());
                // Behavior?.RenderCellShadow(canvas, offset, cell);
                canvas.PopState();
            }

            return;
        }

        Prototype.Model.RenderShadow(canvas, offset);
    }

    public override void FinalizeDeserialization()
    {
        base.FinalizeDeserialization();

        foreach (var cell in this.GetAdjacentCells())
        {
            var structure = Grid.GetCell(Location + cell)?.Structure.Actor;
            if (structure != null)
            {
                neighbors.Add(structure);
            }
        }
    }

    //public override void Damage()
    //{
    //    health--;
    //    if (health <= 0)
    //        IsDestroyed = true;
    //}

    public override void Tick()
    {
        base.Tick();
        this.Transform = Grid.Transform.Translated(DoubleVector.FromVector2(Location.ToCartesian())).Rotated(Rotation * (MathF.Tau / 6f));

        Enabled = ((Planet)Grid.Parent).NetPower >= 0;
        // Behavior?.Update();
    }

    public override void OnDestroyed()
    {
        var planet = (Planet)Grid.Parent;
        planet.PowerProduced -= Prototype.PowerProduced;
        planet.PowerConsumed -= Prototype.PowerConsumed;

        Grid.RemoveStructure(this);
        base.OnDestroyed();
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);

        writer.Write(Team);
        writer.Write(grid);

        writer.Write(Location.Q);
        writer.Write(Location.R);
        writer.Write(Rotation);
    }

    public virtual void OnNeighborAdded(Structure neighbor)
    {
    }

    public virtual void OnNeighborRemoved(Structure neighbor)
    {

    }
}
