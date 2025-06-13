using ImGuiNET;
using SpaceGame.Commands;
using SpaceGame.Debugging;
using SpaceGame.Extensions;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Orders;

[Serializable(Abstract = true)]
internal abstract class Order
{
    [field: Serialize]
    public required Unit Unit { get; set; }
    public bool IsCompleted { get; private set; } = false;

    public abstract void Tick();

    public virtual void RenderOverlay(ICanvas canvas)
    {
    }

    public bool MoveTo(DoubleVector targetPosition)
    {
        ShipPrototype prototype = (ShipPrototype)Unit.Prototype;
        Ship s = (Ship)Unit;

        var delta = targetPosition - s.Transform.Position;
        double dist = delta.Length();
        double parallelLength = DoubleVector.Dot(s.velocity, delta) / dist;
        DoubleVector parallel = delta.Normalized() * parallelLength;
        DoubleVector perp = s.velocity - parallel;
        // DebugDraw.Ray(Vector2.Zero, parallel.ToVector2(), s.Transform with { Rotation = 0 });
        // DebugDraw.Ray(Vector2.Zero, perp.ToVector2(), s.Transform with { Rotation = 0 });

        // para     perp    angle
        // 1        0       0
        // 0        1       pi/2
        // .5       .5      pi/4

        //if (!TurnTo(Angle.FromVector(delta.ToVector2()) + double.Sign(DoubleVector.Cross(perp, delta)) * float.Atan2((float)perp.Length(), (float)parallel.Length())))
        //{
        //    return false;
        //}

        float targetAngle = Angle.FromVector(delta.ToVector2());
        if (!TurnTo(targetAngle))
        {
            return false;
        }

        float timeToTarget = (float)delta.Length() / (float)s.velocity.Length();
        float timeToStop = (float)s.velocity.Length() / prototype.FlySpeed;

        DoubleVector targetVelocity;
        if (timeToTarget <= timeToStop + Program.Timestep * 10 || perp.LengthSquared() > 0.0001)
        {
            targetVelocity = DoubleVector.Zero;
        }
        else 
        {
            targetVelocity = delta.Normalized() * 1_000_000;
        }

        // DebugDraw.Ray(Vector2.Zero, targetVelocity.ToVector2(), s.Transform with { Rotation = 0 });
        // DebugDraw.Ray(Vector2.Zero, s.velocity.ToVector2(), s.Transform with { Rotation = 0 });
        float accel = s.Prototype.FlySpeed * Program.Timestep;
        s.velocity = DoubleVector.Step(s.velocity, targetVelocity, accel);
        
        //if (DoubleVector.Dot(delta, s.velocity) > 0 && timeToTarget < timeToStop)
        //{
        //    s.Fly(-1);
        //}
        //else
        //{
        //    if (delta.Length() > 1)
        //    {
        //        s.velocity = DoubleVector.Step(s.velocity, delta, accel);
        //    }
        //    else
        //    {
        //        s.velocity = DoubleVector.Step(s.velocity, delta, accel);
        //    }
        //        //DoubleVector.Step(delta.Normalized(),, );
        //        // MathHelper.Step();
        //}

            // DebugDraw.Text(timeToTarget + " < " + timeToStop, 1, default, s.Transform with { Rotation = 0 }, timeToTarget < timeToStop ? Color.Red : Color.Green );
            // DebugDraw.Ray(default, s.velocity.ToVector2(), s.Transform with { Rotation = 0 });

            //if (Vector2.DistanceSquared(targetPosition.ToVector2(), s.Transform.Position.ToVector2()) > 0.01f)
            //{
            //    if (Angle.Distance(transform.Rotation, Angle.FromVector(delta.ToVector2())) > 0f)
            //    {
            //        transform.Rotation = Angle.Step(transform.Rotation, Angle.FromVector(delta.ToVector2()), prototype.TurnSpeed * MathF.Tau * Program.Timestep);
            //        if (Angle.Distance(transform.Rotation, Angle.FromVector(delta.ToVector2())) > .01f)
            //        {
            //            return false;
            //        }
            //    }

            //    transform.Position = Util.Step(transform.Position, targetPosition, prototype.FlySpeed * Program.Timestep);
            //    return false;
            //}

            //if (targetRotation != null && Angle.Distance(transform.Rotation, targetRotation.Value) > 0f)
            //{
            //    transform.Rotation = Angle.Step(transform.Rotation, targetRotation.Value, prototype.TurnSpeed * MathF.Tau * Program.Timestep);
            //    return false;
            //}

        return DoubleVector.Distance(s.Transform.Position, targetPosition) < 0.05 && s.velocity.Length() <= 0.01;
    }

    [DebugOverlay]
    static bool ShowTurningAngles = false;

    public bool TurnTo(float rotation)
    {
        ShipPrototype prototype = (ShipPrototype)Unit.Prototype;
        Ship s = (Ship)Unit!;

        float delta = Angle.SignedDistance(rotation, s.Transform.Rotation);

        float turnSpeedRadians = prototype.TurnSpeed * MathF.Tau;

        float timeToStop = MathF.Abs(s.angularVelocity) / turnSpeedRadians;
        float timeToTarget = MathF.Abs(delta / s.angularVelocity);

        if (timeToTarget <= timeToStop)
        {
            s.Rotate(-MathF.Sign(s.angularVelocity));
        }
        else
        {
            s.Rotate(MathF.Sign(delta));
        }

        if (ShowTurningAngles)
        {
            Color c = timeToTarget <= timeToStop ? Color.Red : Color.Green;
            DebugDraw.Ray(Vector2.Zero, Angle.ToVector(delta), s.Transform, c);
            DebugDraw.Ray(Vector2.Zero, Vector2.UnitX, s.Transform, c);
        }

        return Angle.Distance(s.Transform.Rotation, rotation) < 0.1 && float.Abs(s.angularVelocity) <= 0.01;
    }

    public bool MoveToOld(DoubleVector targetPosition, float? targetRotation = null)
    {
        ShipPrototype prototype = (ShipPrototype)Unit.Prototype;

        ref Transform transform = ref Unit.Transform;

        var delta = targetPosition - transform.Position;

        if (Vector2.DistanceSquared(targetPosition.ToVector2(), transform.Position.ToVector2()) > 0.01f)
        {
            if (Angle.Distance(transform.Rotation, Angle.FromVector(delta.ToVector2())) > 0f)
            {
                transform.Rotation = Angle.Step(transform.Rotation, Angle.FromVector(delta.ToVector2()), prototype.TurnSpeed * MathF.Tau * Program.Timestep);
                if (Angle.Distance(transform.Rotation, Angle.FromVector(delta.ToVector2())) > .01f)
                {
                    return false;
                }
            }

            transform.Position = Util.Step(transform.Position, targetPosition, prototype.FlySpeed * Program.Timestep);
            return false;
        }

        if (targetRotation != null && Angle.Distance(transform.Rotation, targetRotation.Value) > 0f)
        {
            transform.Rotation = Angle.Step(transform.Rotation, targetRotation.Value, prototype.TurnSpeed * MathF.Tau * Program.Timestep);
            return false;
        }

        return true;
    }

    public void Complete()
    {
        IsCompleted = true;
    }

    //public override void Serialize(BinaryWriter writer)
    //{
    //    writer.Write(ID);
    //    writer.Write(Unit);
    //}

}
