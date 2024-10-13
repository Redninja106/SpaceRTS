using SpaceGame.Interaction;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class StructureInstance : UnitBase
{
    public HexCoordinate Location { get; set; }
    public int Rotation { get; set; }
    public Grid Grid { get; set; }
    public Structure Structure { get; set; }
    public StructureBehavior? Behavior { get; set; }
    public List<HexCoordinate>? Footprint { get; set; }

    private Vector2[]? outline;

    public int health = 20;

    public override ref Transform Transform 
    {
        get 
        { 
            base.Transform = Grid.Transform.Translated(Location.ToCartesian()).Rotated(Rotation * (MathF.Tau / 6f));
            return ref base.Transform;
        }
    }

    public StructureInstance(HexCoordinate location, int rotation, Grid grid, Structure structure, Team team, Type? behaviorType, List<HexCoordinate>? footprint)
    {
        Location = location;
        Rotation = rotation;
        Structure = structure;
        Grid = grid;
        this.Team = team;
        this.Footprint = footprint;
        ComputeOutline();

        if (behaviorType != null)
            Behavior = (StructureBehavior)Activator.CreateInstance(behaviorType, this)!;
    }

    public void ComputeOutline()
    {
        if (Footprint != null)
        {
            outline = Structure.CreateOutline(Footprint);
        }
    }

    public override bool TestPoint(Vector2 point, Transform transform)
    {
        Vector2 localPoint = Grid.Transform.WorldToLocal(transform.LocalToWorld(point));
        HexCoordinate coord = HexCoordinate.FromCartesian(localPoint);
        return Grid.GetCell(coord) is GridCell cell && cell.Structure == this;
    }

    public override void Render(ICanvas canvas)
    {
        bool isSelected = World.SelectionHandler.IsSelected(this);
        if (isSelected)
        {
            var outline = this.outline ?? Structure.Outline;

            for (int i = 0; i < outline.Length; i += 2)
            {
                canvas.Stroke(Team.GetRelationColor(World.PlayerTeam));
                canvas.StrokeWidth(0);
                canvas.DrawLine(outline[i], outline[i + 1]);
            }
        }

        if (Structure is ZoneStructure zone)
        {
            if (!isSelected && ((World.SelectionHandler.GetSelectedObject() as Ship)?.modules?.Any(m => m is ConstructionModule) ?? false))
            {
                canvas.Fill(zone.Color with { A = .5f });
                foreach (var cell in Footprint ?? Structure.Footprint)
                {
                    canvas.PushState();
                    canvas.Translate(cell.ToCartesian());
                    canvas.DrawPolygon(Grid.hexagon);
                    canvas.PopState();
                }
            }

            if (Behavior != null)
            {
                Behavior?.RenderBeforeCells(canvas);

                foreach (var cell in Footprint ?? Structure.Footprint)
                {
                    canvas.PushState();
                    canvas.Translate(cell.ToCartesian());
                    Behavior?.RenderCell(canvas, cell);
                    canvas.PopState();
                }

                Behavior?.RenderAfterCells(canvas);
            }
        }
        else
        {
            Structure.Model.Render(canvas);
        }
    }

    public void RenderShadow(ICanvas canvas, Vector2 offset)
    {
        if (Structure is ZoneStructure zone)
        {
            foreach (var cell in Footprint ?? Structure.Footprint)
            {
                canvas.PushState();
                canvas.Translate(cell.ToCartesian());
                Behavior?.RenderCellShadow(canvas, offset, cell);
                canvas.PopState();
            }

            return;
        }

        Structure.Model.RenderShadow(canvas, offset);
    }

    public override void Damage()
    {
        health--;
        if (health <= 0)
            IsDestroyed = true;
    }

    public override void Update()
    {
        Behavior?.Update();
    }
}
