using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Orders;
internal class AttackOrder : Order
{
    public ActorReference<Unit> target;

    public AttackOrder(AttackOrderPrototype prototype, ulong id, Transform transform, ActorReference<Unit> target) : base(prototype, id, transform)
    {
        this.target = target;
    }

    public override bool Complete(Ship ship)
    {
        return target.Actor!.Health > 0;
    }

    public override void Render(Ship ship, ICanvas canvas)
    {
        canvas.Stroke(Color.Red);
        canvas.DrawLine(ship.Transform.Position, target.Actor!.Transform.Position);

        base.Render(ship, canvas);
    }

    public override void Serialize(BinaryWriter writer)
    {
    }
}
