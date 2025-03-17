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
    public DoubleVector target;

    public MoveCommand(MoveCommandPrototype prototype, Ship ship, DoubleVector target) : base(prototype)
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
        DoubleVector v = reader.ReadDoubleVector();

        return new MoveCommand(this, s.Actor!, v);
    }

    public override void Issue(Unit? target, HashSet<Unit> selected, PlayerCommandProcessor processor)
    {
        foreach (var unit in selected)
        {
            DoubleVector targetPosition;
            if (target == null)
            {
                targetPosition = World.MousePosition;
            }
            else
            {
                targetPosition = target.Transform.Position;
            }

            processor.AddCommand(new MoveCommand(this, (Ship)unit!, targetPosition));
        }
    }

    public override bool Applies(Unit? target, HashSet<Unit> selected)
    {
        return selected.Count > 0;
    }
}
