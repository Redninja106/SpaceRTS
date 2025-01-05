using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Orders;
internal class MoveOrder : Order
{
    public Vector2 target;

    public MoveOrder(MoveOrderPrototype prototype, ulong id, ActorReference<Unit> unit, Vector2 target) : base(prototype, id, unit)
    {
        this.target = target;
    }

    public override void Tick()
    {
        var soi = World.GetSphereOfInfluence(target);
        soi?.ApplyTo(ref target);

        if (MoveTo(target))
        {
            Complete();
        }
    }


    public override void Render(ICanvas canvas)
    {
        canvas.PopState();
        canvas.Stroke(Color.White with { A = 200 });
        Vector2 shipPos = Unit.Actor!.Transform.Position;
        canvas.DrawLine(shipPos, target);
        canvas.PushState();
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(target);
    }
}
