using SpaceGame.Extensions;
using SpaceGame.GUI;
using SpaceGame.Interaction;
using SpaceGame.Planets;
using SpaceGame.Rendering;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;

[Serializable]
internal class Structure : Unit
{
    public override StructurePrototype Prototype => (StructurePrototype)base.Prototype;

    [field: Serialize]
    public required HexCoordinate Location { get; set; }
    [field: Serialize]
    public required int Rotation { get; set; }
    [field: Serialize]
    public required Grid Grid { get; set; }
    
    public List<HexCoordinate>? Footprint { get; set; }
    public HashSet<Structure> neighbors = [];
    public bool Enabled { get; set; }
    public bool Powered { get; set; }
    // public override ITexture Icon => Icons.Structure;

    public Structure(StructurePrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
        UpdateStatus();
    }

    public override void InitializeActor()
    {
        this.Teleport(Grid.Transform.Translated(DoubleVector.FromVector2(this.Location.ToCartesian())).Rotated(Rotation * (MathF.Tau / 6f)));

        foreach (var unlock in Prototype.Unlocks)
        {
            this.Team.Unlock(unlock);
        }

        base.InitializeActor();
    }

    public override DoubleVector GetCenter()
    {
        return this.Transform.Position + DoubleVector.FromVector2(this.Prototype.Center.Rotated(this.Rotation * MathF.PI / 6f));
    }

    public IEnumerable<HexCoordinate> GetAdjacentCells()
    {
        foreach (var cell in Prototype.AdjacentCells)
        {
            yield return this.Location + cell.Rotated(this.Rotation);
        }
    }

    [DebugOverlay]
    public static void ShowStructureAdjacentCells()
    {
        if (Program.World.SelectInteractionContext.target is Structure structure)
        {
            foreach (var adjacent in structure.GetAdjacentCells())
            {
                DebugDraw.Circle(adjacent.ToCartesian(), 1, structure.Grid.Transform);
            }
        }
    }

    public override bool TestPoint(DoubleVector point, bool interpolated = false)
    {
        Transform transform = interpolated ? Grid.InterpolatedTransform : Grid.Transform;

        Vector2 localPoint = transform.WorldToLocal(point.ToVector2());
        HexCoordinate coord = HexCoordinate.FromCartesian(localPoint);
        return Grid.GetCell(coord) is GridCell cell && cell.Structure == this;
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Translate(Prototype.Center);
        canvas.Rotate(-(this.Rotation * MathF.Tau / 6f));
        Prototype.Model.Render(canvas, this.InterpolatedTransform, ColorF.White);

        if (!Powered)
        {
            Prototype.Model.Render(canvas, this.InterpolatedTransform, new ColorF(.5f, .5f, .5f, 1));
            ITexture icon = Rendering.Icon.Get("economic_icon").Texture64x64;
            canvas.DrawTexture(icon, new Rectangle(0, 0, 2, 2, Alignment.Center), ColorF.Red);
        }
    }

    public void RenderShadow(ICanvas canvas, Vector2 offset)
    {
        foreach (var cell in Prototype.Footprint)
        {
            canvas.PushState();
            canvas.Translate(cell.ToCartesian());
            // Behavior?.RenderCellShadow(canvas, offset, cell);
            canvas.PopState();
        }

        // Prototype.Model.RenderShadow(canvas, offset);
    }

    public override void FinishDeserialization()
    {
        base.FinishDeserialization();

        foreach (var cell in this.GetAdjacentCells())
        {
            var structure = Grid.GetCell(Location + cell)?.Structure;
            if (structure != null)
            {
                neighbors.Add(structure);
            }
        }
    }

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
        // planet.PowerProduced -= Prototype.PowerProduced;
        // planet.PowerConsumed -= Prototype.PowerConsumed;

        Grid.RemoveStructure(this);
        base.OnDestroyed();
    }

    //public override void Serialize(BinaryWriter writer)
    //{
    //    writer.Write(ID);

    //    writer.Write(Team);
    //    writer.Write(grid);

    //    writer.Write(Location);
    //    writer.Write(Rotation);
    //}

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

    public override void RenderBackgroundOverlay(ICanvas canvas, Camera camera, bool selected)
    {
        for (int i = 0; i < Prototype.Outline.Length; i += 2)
        {
            Vector2 from = Prototype.Outline[i];
            Vector2 to = Prototype.Outline[i + 1];

            Vector2 edgeCenter = (to + from) / 2f;

            Vector2 delta = to - from;
            Vector2 side1 = this.Location.ToCartesian() + edgeCenter + new Vector2(delta.Y, -delta.X);
            Vector2 side2 = this.Location.ToCartesian() + edgeCenter + new Vector2(-delta.Y, delta.X);

            var structure1 = this.Grid.GetCell(HexCoordinate.FromCartesian(side1))?.Structure;
            var structure2 = this.Grid.GetCell(HexCoordinate.FromCartesian(side2))?.Structure;

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


        // base.DrawHighlightAbove(canvas, camera, selected);
    }

    public override void RenderGroundOverlay(ICanvas canvas, Camera camera, bool selected)
    {
        canvas.Transform(World.Camera.CreateRelativeMatrix(InterpolatedTransform));
        canvas.Stroke(World.PlayerTeam.GetRelationColor(Team) with { A = (byte)(selected ? 255 : 100) });
        for (int i = 0; i < Prototype.Outline.Length; i += 2)
        {
            canvas.DrawLine(Prototype.Outline[i], Prototype.Outline[i + 1]);
        }

        // base.DrawHighlightBelow(canvas, camera, selected);
    }

}
