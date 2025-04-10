﻿using SpaceGame;
using SpaceGame.Commands;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Orders;
internal abstract class Order(OrderPrototype prototype, ulong id, ActorReference<Unit> unit) : WorldActor(prototype, id, Transform.Default)
{
    public ActorReference<Unit> Unit { get; } = unit;

    public bool IsCompleted { get; private set; } = false;

    public bool MoveTo(DoubleVector targetPosition, float? targetRotation = null)
    {
        ShipPrototype prototype = (ShipPrototype)Unit.Actor!.Prototype;

        ref Transform transform = ref Unit.Actor!.Transform;

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

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);
        writer.Write(Unit);
    }

}
