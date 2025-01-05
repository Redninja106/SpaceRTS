using SpaceGame.Orders;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
internal class AttackCommand : Command
{
    ActorReference<Unit> unit;
    ActorReference<Unit> target;

    public AttackCommand(CommandPrototype prototype, ActorReference<Unit> unit, ActorReference<Unit> target) : base(prototype)
    {
        this.unit = unit;
        this.target = target;
    }

    public override void Apply()
    {
        var order = new AttackOrder(
            Prototypes.Get<AttackOrderPrototype>("attack_order"),
            World.NewID(),
            unit,
            target
            );

        if (unit.Actor is Ship s)
        {
            s.orders.Enqueue(order.AsReference().Cast<Order>());
        }
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(unit);
        writer.Write(target);
    }
}

class AttackCommandPrototype : CommandPrototype
{
    public override Actor Deserialize(BinaryReader reader)
    {
        var unit = reader.ReadActorReference<Unit>();
        var target = reader.ReadActorReference<Unit>();
        return new AttackCommand(this, unit, target);
    }
}
