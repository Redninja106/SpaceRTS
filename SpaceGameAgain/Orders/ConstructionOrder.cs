using SpaceGame.Ships;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Orders;
internal class ConstructionOrder : Order
{
    public ActorReference<Grid> Grid;
    public HexCoordinate Location;
    public int Rotation;
    public StructurePrototype Structure;

    public ConstructionOrder(ConstructionOrderPrototype prototype, ulong id, ActorReference<Unit> unit, ActorReference<Grid> grid, StructurePrototype structure, HexCoordinate location, int rotation) : base(prototype, id, unit)
    {
        Grid = grid;
        Structure = structure;
        Location = location;
        Rotation = rotation;
    }

    public override void Tick()
    {
        if (!MoveTo(FixedVector2.FromVector2(Grid.Actor!.Transform.LocalToWorld(Location.ToCartesian()))))
        {
            return;
        }

        if (!Grid.Actor!.IsStructureObstructed(Structure, Location, Rotation))
        {
            Grid.Actor!.PlaceStructure(Structure, Location, Rotation, Unit.Actor!.Team.Actor!);
            Unit.Actor!.Team.Actor!.Credits -= Structure.Price;
            Complete();
        }
    }

    public override void Render(ICanvas canvas)
    {
        Grid.Actor!.Transform.ApplyTo(canvas);
        canvas.Translate(Location.ToCartesian());
        canvas.Rotate(Rotation * (MathF.Tau / 6f));
        Structure.Model.Render(canvas, 100);

        base.Render(canvas);
    }


    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(Grid);
        writer.Write(Structure.Name);
        writer.Write(Location);
        writer.Write(Rotation);
    }
}
