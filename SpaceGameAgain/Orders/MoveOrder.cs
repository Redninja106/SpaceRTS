using SpaceGame.Extensions;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Orders;

[Serializable]
internal class MoveOrder : Order
{
    [Serialize]
    public required DoubleVector target;

    public override void Tick()
    {
        var soi = World.GetSphereOfInfluence(target);
        if (soi != null)
        {
            target = soi.ApplyTickTo(target);
        }
        if (MoveTo(target))
        {
            Complete();
        }
    }

    //public override void Update(float tickProgress)
    //{
    //    base.Update(tickProgress);
    //}

    public override void RenderOverlay(ICanvas canvas)
    {
        canvas.PushState();
        canvas.ResetState();

        Transform t = Transform.Default with
        {
            Position = target,
        };
        t.ApplyTo(canvas, World.Camera);

        canvas.Stroke(Color.White with { A = 200 });
        canvas.DrawLine(Vector2.Zero, (Unit.InterpolatedTransform.Position - t.Position).ToVector2());
        canvas.PopState();
    }

    //public override void Serialize(BinaryWriter writer)
    //{
    //    base.Serialize(writer);
    //    writer.Write(this.Transform.Position);
    //}
}