﻿using Silk.NET.Input;
using SpaceGame.GUI;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class ChaingunSystem(ChaingunSystemPrototype prototype, ulong id, ActorReference<Unit> unit) : WeaponSystem(prototype, id, unit)
{
    public float fireRate = 17;
    public float range = 6;
    public int ammoCapacity = 150;
    public float turnSpeed = MathF.Tau;

    public int ammo = 150;
    public float angle;
    public float timeSinceShot;

    public override void Tick()
    {
        base.Tick();

        Missile? target = null;
        float minDistance = float.PositiveInfinity;
        foreach (var missile in World.Missiles)
        {
            if (DoubleVector.Distance(missile.Transform.Position, unit.Actor!.Transform.Position) <= range && missile.Target.Actor!.Team.Actor!.GetRelation(unit.Actor!.Team.Actor!) is TeamRelation.Allies or TeamRelation.Self)
            {
                if (missile.exploding)
                    continue;

                float dist = DoubleVector.Distance(missile.Transform.Position, unit.Actor!.Transform.Position);
                if (dist < minDistance)
                {
                    target = missile;
                    minDistance = dist;
                }
            }
        }

        if (target is not null)
        {
            if (timeSinceShot > 1 / fireRate && ammo > 0)
            {
                var bulletProto = Prototypes.Get<BulletPrototype>("bullet");
                DoubleVector targetPos = target.Transform.Position;
                DoubleVector position = target.Transform.Position;
                DoubleVector velocity = target.Velocity;
                DoubleVector acceleration = target.CurrentAcceleration;
                DoubleVector jerk = (target.CurrentAcceleration - target.LastAcceleration) / Program.Timestep;

                for (int i = 0; i < 8; i++)
                {
                    targetPos = PredictBullet(unit.Actor!.Transform.Position, targetPos, bulletProto.Speed, position, velocity, acceleration, jerk, 1);
                    // DebugDraw.Circle(targetPos, 0.01f * (8f-i) / 10f, color: Color.FromHSV((this.ID * 123.45f) % 1f, 1, 1));
                }

                float targetAngle = Angle.FromVector((targetPos - unit.Actor!.Transform.Position).ToVector2());
                angle = Angle.Step(angle, targetAngle, turnSpeed * Program.Timestep);
                
                if (Angle.Distance(angle, targetAngle) < 0.05f)
                {
                    var transform = unit.Actor!.Transform with 
                    { 
                        Rotation = Angle.FromVector((targetPos - unit.Actor!.Transform.Position).ToVector2()) + World.TickRandom.NextSingle() * 0.05f
                    };
                    World.Add(new Bullet(bulletProto, World.NewID(), transform, target.AsReference(), range / bulletProto.Speed));

                    timeSinceShot = 0;
                    ammo--;
                }
            }
        }

        if (timeSinceShot > 3)
        {
            ammo = ammoCapacity;
        }

        timeSinceShot += Program.Timestep;
    }

    public void RenderSelected(ICanvas canvas)
    {
        canvas.Stroke(Color.Green);
        canvas.DrawCircle(0, 0, range);
    }


    public override void Render(ICanvas canvas)
    {
        base.Render(canvas);
        // DebugDraw.Line(Vector2.Zero, Vector2.UnitX, this.unit.Actor!.Transform with { Rotation = angle } );
        // canvas.DrawLine(Vector2.Zero, Vector2.UnitX);
    }

    private DoubleVector PredictBullet(DoubleVector turretPos, DoubleVector targetPos, float bulletSpeed, DoubleVector position, DoubleVector velocity, DoubleVector acceleration, DoubleVector jerk, float minTimeToHit)
    {
        DoubleVector delta = targetPos - turretPos;
        double angle = (float)Angle.Distance(Angle.FromVector(delta.ToVector2()), this.angle);
        double distance = delta.Length();
        double t = Math.Min((distance / bulletSpeed + 0 * angle / turnSpeed), 1);
        return Forecast(position, velocity, acceleration, jerk, t);
    }

    public DoubleVector Forecast(DoubleVector p, DoubleVector v, DoubleVector a, DoubleVector j, double t)
    {
        return p + v * t + (1 / 2f) * a * t * t + (1 / 6f) * j * t * t * t;
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);
        writer.Write(unit);
        writer.Write(ammo);
        writer.Write(angle);
        writer.Write(timeSinceShot);
    }
}