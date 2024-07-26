using SpaceGame.Interaction;
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

    public int health = 20;

    public override ref Transform Transform 
    {
        get 
        { 
            base.Transform = Grid.Transform.Translated(Location.ToCartesian()).Rotated(Rotation * (MathF.Tau / 6f));
            return ref base.Transform;
        }
    }

    public StructureInstance(HexCoordinate location, int rotation, Grid grid, Structure structure, Team team, Type? behaviorType)
    {
        Location = location;
        Rotation = rotation;
        Structure = structure;
        Grid = grid;
        this.Team = team;
        
        if (behaviorType != null)
            Behavior = (StructureBehavior)Activator.CreateInstance(behaviorType, this)!;
    }

    public override bool TestPoint(Vector2 point, Transform transform)
    {
        Vector2 localPoint = Grid.Transform.WorldToLocal(transform.LocalToWorld(point));
        HexCoordinate coord = HexCoordinate.FromCartesian(localPoint);
        return Grid.GetCell(coord) is GridCell cell && cell.Structure == this;
    }

    public override void Render(ICanvas canvas)
    {
        if (World.SelectionHandler.IsSelected(this))
        {
            for (int i = 0; i < Structure.Outline.Length; i += 2)
            {
                canvas.Stroke(Team.GetRelationColor(World.PlayerTeam));
                canvas.StrokeWidth(0);
                canvas.DrawLine(Structure.Outline[i], Structure.Outline[i + 1]);
            }
        }

        Structure.Model.Render(canvas);
    }

    public void RenderShadow(ICanvas canvas, Vector2 offset)
    {
        Structure.Model.RenderShadow(canvas, offset);
    }

    public override void Damage()
    {
        health--;
        if (health <= 0)
            IsDestroyed = true;
    }

    public override void Update(float tickProgress)
    {
        Behavior?.Update();
    }
}
