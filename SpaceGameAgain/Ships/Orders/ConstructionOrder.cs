using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Orders;
internal class ConstructionOrder(ConstructionOrderPrototype prototype, ulong id, Transform transform) : Order(prototype, id, transform)
{
    public StructurePrototype Structure;
    public Grid Grid;
    public HexCoordinate Location;
    public int Rotation;

    public override bool Complete(Ship ship)
    {
        if (!MoveTo(ship, Grid.Transform.LocalToWorld(Location.ToCartesian())))
            return false;

        if (!Grid.IsStructureObstructed(Structure, Location, Rotation))
        {
            Grid.PlaceStructure(Structure, Location, Rotation, ship.Team.Actor!);
            ship.Team.Actor!.Credits -= Structure.Price;
        }
        return true;
    }

    public override void Render(Ship ship, ICanvas canvas)
    {
        Grid.Transform.ApplyTo(canvas);
        canvas.Translate(Location.ToCartesian());
        canvas.Rotate(Rotation * (MathF.Tau / 6f));
        Structure.Model.Render(canvas, 100);

        base.Render(ship, canvas);
    }

    public override void Serialize(BinaryWriter writer)
    {
    }
}
