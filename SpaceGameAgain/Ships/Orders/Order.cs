using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Orders;
internal abstract class Order
{
    public abstract bool Complete(Ship ship);

    public virtual void Render(Ship ship, ICanvas canvas)
    {
    }

    public bool MoveTo(Ship ship, Vector2 targetPosition, float? targetRotation = null)
    {
        var delta = targetPosition - ship.Transform.Position;

        if (Vector2.DistanceSquared(targetPosition, ship.Transform.Position) > 0.01f)
        {
            if (Angle.Distance(ship.Transform.Rotation, Angle.FromVector(delta)) > 0f)
            {
                ship.Transform.Rotation = Angle.Step(ship.Transform.Rotation, Angle.FromVector(delta), MathF.Tau * Time.DeltaTime);
                if (Angle.Distance(ship.Transform.Rotation, Angle.FromVector(delta)) > .01f)
                    return false;
            }

            ship.Transform.Position = Util.Step(ship.Transform.Position, targetPosition, ship.speed * Time.DeltaTime);
            return false;
        }

        if (targetRotation != null && Angle.Distance(ship.Transform.Rotation, targetRotation.Value) > 0f)
        {
            ship.Transform.Rotation = Angle.Step(ship.Transform.Rotation, targetRotation.Value, MathF.Tau * Time.DeltaTime);
            return false;
        }

        return true;
    }
}
