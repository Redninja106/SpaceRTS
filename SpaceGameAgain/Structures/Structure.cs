using SpaceGame.Extensions;
using SpaceGame.GUI;
using SpaceGame.Interaction;
using SpaceGame.Planets;
using SpaceGame.Rendering;
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

    private ActorReference<Grid> grid;

    public HashSet<Structure> neighbors = [];

    public bool Enabled { get; set; }
    public bool Powered { get; set; }
    public override ITexture Icon => Icons.Structure;

    public Structure(StructurePrototype prototype, ulong id, ActorReference<Grid> grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid.Actor!.Transform.Translated(DoubleVector.FromVector2(location.ToCartesian())).Rotated(rotation * (MathF.Tau / 6f)), team)
    {
        Location = location;
        Rotation = rotation;
        this.grid = grid;
        UpdateStatus();
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

    public override DoubleVector GetCenter()
    {
        return this.Transform.Position + DoubleVector.FromVector2(this.Prototype.Center.Rotated(this.Rotation * MathF.PI / 6f));
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

    public override bool TestPoint(DoubleVector point)
    {
        Vector2 localPoint = Grid.Transform.WorldToLocal(point.ToVector2());
        HexCoordinate coord = HexCoordinate.FromCartesian(localPoint);
        return Grid.GetCell(coord) is GridCell cell && cell.Structure.Actor == this;
    }

    public override void Render(ICanvas canvas)
    {
        if (Prototype is ZonedStructurePrototype zone)
        {
            //if (!isSelected && ((World.SelectionHandler.GetSelectedUnit() as Ship)?.modules?.Any(m => m is ConstructionModule) ?? false))
            //{
            //    canvas.Fill((zone.Color with { A = .5f }));
            //    foreach (var cell in Prototype.Footprint)
            //    {
            //        canvas.PushState();
            //        canvas.Translate(cell.ToCartesian());
            //        canvas.DrawPolygon(Grid.hexagon);
            //        canvas.PopState();
            //    }
            //}

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
            canvas.Translate(Prototype.Center);
            canvas.Rotate(-(this.Rotation * MathF.Tau / 6f));
            Prototype.Model.Render(canvas, this.InterpolatedTransform, ColorF.White);

            if (!Powered)
            {
                Prototype.Model.Render(canvas, this.InterpolatedTransform, new ColorF(.5f, .5f, .5f, 1));
                canvas.DrawTexture(Icons.Economic, new Rectangle(0, 0, 2, 2, Alignment.Center), ColorF.Red);
            }
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

        // Prototype.Model.RenderShadow(canvas, offset);
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
        UpdateStatus();

        //Enabled = ((Planet)Grid.Parent).NetPower >= 0;
        // Behavior?.Update();
    }

    private void UpdateStatus()
    {
        if (Prototype.RequiredPowerLevel > Economy.PowerLevel.None)
        {
            Powered = neighbors.Any(n => n.Team == this.Team && n.Prototype.ProvidedPowerLevel >= this.Prototype.RequiredPowerLevel);
        }
        else
        {
            Powered = true;
        }
        Enabled = Powered;
    }

    public override void OnDestroyed()
    {
        var planet = (Planet)Grid.Parent;
        // planet.PowerProduced -= Prototype.PowerProduced;
        // planet.PowerConsumed -= Prototype.PowerConsumed;

        Grid.RemoveStructure(this);
        base.OnDestroyed();
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);

        writer.Write(Team);
        writer.Write(grid);

        writer.Write(Location);
        writer.Write(Rotation);
    }

    public virtual void OnNeighborAdded(Structure neighbor)
    {
    }

    public virtual void OnNeighborRemoved(Structure neighbor)
    {
    }

    public override void Layout(GUIWindow window)
    {
        // window.Text(Prototype.Title);
    }

    public override void DrawHighlightAbove(ICanvas canvas, Camera camera, bool selected)
    {
        for (int i = 0; i < Prototype.Outline.Length; i += 2)
        {
            Vector2 from = Prototype.Outline[i];
            Vector2 to = Prototype.Outline[i + 1];

            Vector2 edgeCenter = (to + from) / 2f;

            Vector2 delta = to - from;
            Vector2 side1 = this.Location.ToCartesian() + edgeCenter + new Vector2(delta.Y, -delta.X);
            Vector2 side2 = this.Location.ToCartesian() + edgeCenter + new Vector2(-delta.Y, delta.X);

            var structure1 = this.grid.Actor!.GetCell(HexCoordinate.FromCartesian(side1))?.Structure.Actor;
            var structure2 = this.grid.Actor!.GetCell(HexCoordinate.FromCartesian(side2))?.Structure.Actor;

            if (structure1 != null && structure2 != null && structure1.Team == structure2.Team)
            {
                canvas.PushState();
                this.InterpolatedTransform.WithRotation(0).ApplyTo(canvas, camera);
                structure1.Prototype.RenderAdjacencyOverlay(canvas, edgeCenter, structure2.Prototype);
                structure2.Prototype.RenderAdjacencyOverlay(canvas, edgeCenter, structure1.Prototype);
                canvas.PopState();
            }
        }

        //foreach (var neighbor in neighbors)
        //{
        //    canvas.PushState();
        //    Transform transform = Transform.Default with
        //    {
        //        Position = (this.GetCenter() + neighbor.GetCenter()) / 2
        //    }; 
        //    transform.ApplyTo(canvas, camera);
        //    neighbor.Prototype.RenderAdjacencyOverlay(canvas, Vector2.Zero, this.Prototype);
        //    this.Prototype.RenderAdjacencyOverlay(canvas, Vector2.Zero, neighbor.Prototype);
        //    canvas.PopState();
        //}


        base.DrawHighlightAbove(canvas, camera, selected);
    }

    public override void DrawHighlightBelow(ICanvas canvas, Camera camera, bool selected)
    {
        canvas.Transform(World.Camera.CreateRelativeMatrix(InterpolatedTransform));
        canvas.Stroke(World.PlayerTeam.Actor!.GetRelationColor(Team.Actor!) with { A = (byte)(selected ? 255 : 100) });
        for (int i = 0; i < Prototype.Outline.Length; i += 2)
        {
            canvas.DrawLine(Prototype.Outline[i], Prototype.Outline[i + 1]);
        }

        base.DrawHighlightBelow(canvas, camera, selected);
    }

}
