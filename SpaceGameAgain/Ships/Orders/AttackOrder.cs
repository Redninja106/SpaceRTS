using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Orders;
internal class AttackOrder : Order
{
    public UnitBase target;

    public AttackOrder(UnitBase target)
    {
        this.target = target;
    }

    public override bool Complete(Ship ship)
    {
        return target.IsDestroyed;
    }

    public override void Render(Ship ship, ICanvas canvas)
    {
        canvas.Stroke(Color.Red);
        canvas.DrawLine(ship.Transform.Position, target.Transform.Position);

        base.Render(ship, canvas);
    }
}
