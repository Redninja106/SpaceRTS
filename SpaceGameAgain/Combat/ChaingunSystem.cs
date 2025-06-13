using Silk.NET.Input;
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
internal class ChaingunSystem(ChaingunSystemPrototype prototype, ulong id) : WeaponSystem(prototype, id)
{
    public override ChaingunSystemPrototype Prototype => (ChaingunSystemPrototype)base.Prototype;

    public int ammo = 150;
    public int timeSinceShot;

    public override void Tick()
    {
        base.Tick();

        ITargetable? target = null;
        float minDistance = float.PositiveInfinity;
        foreach (var missile in World.Missiles)
        {
            if (missile.Target == null)
                continue;

            if (DoubleVector.Distance(missile.Transform.Position, this.Transform.Position) <= Prototype.Range && missile.Target.Team.GetRelation(Unit.Team) is TeamRelation.Allies or TeamRelation.Self)
            {
                if (missile.exploding)
                    continue;

                float dist = DoubleVector.Distance(missile.Transform.Position, this.Transform.Position);
                if (dist < minDistance)
                {
                    target = missile;
                    minDistance = dist;
                }
            }
        }

        minDistance = float.PositiveInfinity;
        foreach (var ship in World.Ships)
        {
            if (ship.Team == this.Unit.Team)
                continue;

            if (DoubleVector.Distance(ship.Transform.Position, this.Transform.Position) <= Prototype.Range)
            {
                float dist = DoubleVector.Distance(ship.Transform.Position, this.Transform.Position);
                if (dist < minDistance)
                {
                    target = ship;
                    minDistance = dist;
                }
            }
        }


        if (target is not null)
        {
            if (timeSinceShot > Prototype.FireInterval && ammo > 0)
            {
                var bulletProto = Prototypes.Get<BulletPrototype>("bullet");
                DoubleVector targetPos = target.Transform.Position;
                DoubleVector position = target.Transform.Position;
                DoubleVector velocity = target.Velocity;
                DoubleVector acceleration = target.CurrentAcceleration;
                DoubleVector jerk = (target.CurrentAcceleration - target.LastAcceleration) / Program.Timestep;

                for (int i = 0; i < 8; i++)
                {
                    targetPos = PredictBullet(this.Transform.Position, targetPos, bulletProto.Speed, position, velocity, acceleration, jerk, 1);
                    // DebugDraw.Circle(targetPos, 0.01f * (8f-i) / 10f, color: Color.FromHSV((this.ID * 123.45f) % 1f, 1, 1));
                }

                float targetAngle = Angle.FromVector((targetPos - this.Transform.Position).ToVector2());
                this.Transform.Rotation = Angle.Step(this.Transform.Rotation, targetAngle, Prototype.TurnSpeed * MathF.Tau * Program.Timestep);

                if (Angle.Distance(this.Transform.Rotation, targetAngle) < 0.05f)
                {
                    var transform = this.Transform with 
                    { 
                        Rotation = Angle.FromVector((targetPos - this.Transform.Position).ToVector2()) + World.TickRandom.NextSingle() * 0.05f
                    };

                    Bullet bullet = new(bulletProto, World.NewID())
                    { 
                        target = target, 
                        lifetime = (int)(Program.TickRate * Prototype.Range / bulletProto.Speed)
                    };

                    bullet.Teleport(transform);
                    World.Add(bullet);

                    timeSinceShot = 0;
                    ammo--;
                }
            }
        }

        if (timeSinceShot > Prototype.ReloadTime)
        {
            ammo = Prototype.AmmoCapacity;
        }

        timeSinceShot++;
    }

    public override void Render(ICanvas canvas)
    {
        base.Render(canvas);
        // DebugDraw.Line(Vector2.Zero, Vector2.UnitX, this.unit.Transform with { Rotation = angle } );
        // canvas.DrawLine(Vector2.Zero, Vector2.UnitX);
    }

    private DoubleVector PredictBullet(DoubleVector turretPos, DoubleVector targetPos, float bulletSpeed, DoubleVector position, DoubleVector velocity, DoubleVector acceleration, DoubleVector jerk, float minTimeToHit)
    {
        DoubleVector delta = targetPos - turretPos;
        // double angle = (float)Angle.Distance(Angle.FromVector(delta.ToVector2()), this.angle);
        double distance = delta.Length();
        double t = Math.Min(distance / bulletSpeed /* + 0 * angle / (Prototype.TurnSpeed * float.Tau) */, 1);
        return Forecast(position, velocity, acceleration, jerk, t);
    }

    public static DoubleVector Forecast(DoubleVector p, DoubleVector v, DoubleVector a, DoubleVector j, double t)
    {
        return p + v * t + (1 / 2f) * a * t * t + (1 / 6f) * j * t * t * t;
    }

    //public override void Serialize(BinaryWriter writer)
    //{
    //    writer.Write(ID);
    //    writer.Write(Unit);
    //    writer.Write(ammo);
    //    writer.Write(this.Transform.Rotation);
    //    writer.Write(timeSinceShot);
    //}
}