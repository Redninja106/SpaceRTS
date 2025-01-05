using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Orders;
internal class AttackOrder : Order
{
    public ActorReference<Unit> target;

    public AttackOrder(AttackOrderPrototype prototype, ulong id, ActorReference<Unit> unit, ActorReference<Unit> target) : base(prototype, id, unit)
    {
        this.target = target;
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Stroke(Color.Red);
        canvas.DrawLine(Unit.Actor!.Transform.Position, target.Actor!.Transform.Position);

        base.Render(canvas);
    }


    public override void Tick()
    {
        if (target.Actor!.Health <= 0)
        {
            Complete();
        }
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(target);
    }
}
