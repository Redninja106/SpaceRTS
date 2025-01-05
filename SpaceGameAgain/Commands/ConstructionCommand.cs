using SpaceGame.Orders;
using SpaceGame.Ships;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
internal class ConstructionCommand : Command
{
    public Ship ship;
    public ActorReference<Grid> Grid;
    public HexCoordinate Location;
    public int Rotation;
    public StructurePrototype Structure;

    public ConstructionCommand(CommandPrototype prototype, Ship ship, ActorReference<Grid> grid, HexCoordinate location, int rotation, StructurePrototype structure) : base(prototype)
    {
        this.ship = ship;
        Grid = grid;
        Location = location;
        Rotation = rotation;
        Structure = structure;
    }

    public override void Apply()
    {
        var order = new ConstructionOrder(Prototypes.Get<ConstructionOrderPrototype>("construction_order"), World.NewID(), ship.AsReference().Cast<Unit>(), Grid, Structure, Location, Rotation);
        ship.orders.Enqueue(order.AsReference().Cast<Order>());
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ship.AsReference());
        writer.Write(Grid);

        writer.Write(Location);
        writer.Write(Rotation);
        writer.Write(Structure.Name);
    }
}

class ConstructionCommandPrototype : CommandPrototype
{
    public override Actor Deserialize(BinaryReader reader)
    {
        var ship = reader.ReadActorReference<Ship>();
        var grid = reader.ReadActorReference<Grid>();

        var location = reader.ReadHexCoordinate();
        var rotation = reader.ReadInt32();
        var structure = Prototypes.Get<StructurePrototype>(reader.ReadString());

        return new ConstructionCommand(this, ship.Actor!, grid, location, rotation, structure);
    }
}
