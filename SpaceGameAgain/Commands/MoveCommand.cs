using SpaceGame.Orders;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
internal class MoveCommand : Command
{
    public Ship ship;
    public Vector2 target;

    public MoveCommand(MoveCommandPrototype prototype, Ship ship, Vector2 target) : base(prototype)
    {
        this.ship = ship;
        this.target = target;
    }

    public override void Apply()
    {
        var order = new MoveOrder(Prototypes.Get<MoveOrderPrototype>("move_order"), World.NewID(), ship.AsReference().Cast<Unit>(), target);
        ship.orders.Enqueue(order.AsReference().Cast<Order>());
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ship.AsReference());
        writer.Write(target);
    }
}

class MoveCommandPrototype : CommandPrototype
{
    public override Actor Deserialize(BinaryReader reader)
    {
        ActorReference<Ship> s = reader.ReadActorReference<Ship>();
        Vector2 v = reader.ReadVector2();

        return new MoveCommand(this, s.Actor!, v);
    }
}
