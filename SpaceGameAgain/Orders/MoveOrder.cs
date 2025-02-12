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
    public MoveOrder(MoveOrderPrototype prototype, ulong id, ActorReference<Unit> unit, DoubleVector target) : base(prototype, id, unit)
    {
        Teleport(Transform.Default with { Position = target });
    }

    public override void Tick()
    {
        base.Tick();
        var soi = World.GetSphereOfInfluence(Transform.Position);
        if (soi != null)
        {
            Transform.Position = soi.ApplyTickTo(Transform.Position);
        }
        if (MoveTo(Transform.Position))
        {
            Complete();
        }
    }

    public override void Update(float tickProgress)
    {
        base.Update(tickProgress);
    }

    public override void Render(ICanvas canvas)
    {
        canvas.PushState();
        canvas.ResetState();

        Transform t = Transform.Default with
        {
            Position = this.InterpolatedTransform.Position,
        };
        t.ApplyTo(canvas, World.Camera);

        canvas.Stroke(Color.White with { A = 200 });
        canvas.DrawLine(Vector2.Zero, (Unit.Actor!.InterpolatedTransform.Position - t.Position).ToVector2());
        canvas.PopState();
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(this.Transform.Position);
    }
}