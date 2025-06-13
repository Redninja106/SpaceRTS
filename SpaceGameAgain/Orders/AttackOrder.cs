using SpaceGame.Extensions;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Orders;

[Serializable]
internal class AttackOrder : Order
{
    [Serialize]
    public Unit target;

    public override void RenderOverlay(ICanvas canvas)
    {
        canvas.Stroke(Color.Red);
        canvas.DrawRect(0, 0, 1, 1, Alignment.Center);
        canvas.DrawLine(Unit.Transform.Position.ToVector2(), target.Transform.Position.ToVector2());
    }


    public override void Tick()
    {
        if (target.Health <= 0)
        {
            Complete();
        }
    }

}
