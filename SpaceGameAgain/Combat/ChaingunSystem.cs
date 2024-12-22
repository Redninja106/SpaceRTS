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
internal class ChaingunSystem(ChaingunSystemPrototype prototype, ulong id, Transform transform, Unit unit) : WeaponSystem(prototype, id, transform)
{
    public float fireRate = 17;
    public float range = 4;
    public int ammoCapacity = 150;
    public float turnSpeed = MathF.Tau;

    public int ammo = 150;
    private float angle;
    private float timeSinceShot;

    public override void Update()
    {
        turnSpeed = MathF.Tau;
        Missile? target = null;
        float minDistance = float.PositiveInfinity;
        foreach (var missile in World.Missiles)
        {
            if (Vector2.Distance(missile.Transform.Position, unit.Transform.Position) <= range && missile.Target.Actor!.Team.Actor!.GetRelation(unit.Team!.Actor) is TeamRelation.Allies or TeamRelation.Self)
            {
                if (missile.exploding)
                    continue;

                float dist = Vector2.Distance(missile.Transform.Position, unit.Transform.Position);
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
                float speed = 10;
                Vector2 targetPos = target.Transform.Position;
                Vector2 position = target.Transform.Position;
                Vector2 velocity = target.Velocity;
                Vector2 acceleration = target.CurrentAcceleration;
                Vector2 jerk = (target.CurrentAcceleration - target.LastAcceleration) / Time.DeltaTime;

                for (int i = 0; i < 8; i++)
                {
                    targetPos = PredictBullet(unit.Transform.Position, targetPos, speed, position, velocity, acceleration, jerk, 1);
                }

                float targetAngle = Angle.FromVector(targetPos - unit.Transform.Position);
                angle = Angle.Step(angle, targetAngle, turnSpeed * Time.DeltaTime);
                if (Angle.Distance(angle, targetAngle) < 0.05f)
                {
                    var soi = World.GetSphereOfInfluence(unit.Transform.Position);
                    var transform = unit.Transform with { Rotation = Angle.FromVector(targetPos - unit.Transform.Position) + .01f * MathF.Sin(Time.TotalTime * 50) };
                    World.Add(new Bullet(Prototypes.Get<BulletPrototype>("bullet"), World.NewID(), transform, target, soi, speed, range / speed));

                    timeSinceShot = 0;
                    ammo--;
                }
            }
        }

        if (timeSinceShot > 3)
        {
            ammo = ammoCapacity;
        }

        timeSinceShot += Time.DeltaTime;
    }

    public void RenderSelected(ICanvas canvas)
    {
        canvas.Stroke(Color.Green);
        canvas.DrawCircle(0, 0, range);
    }

    private Vector2 PredictBullet(Vector2 turretPos, Vector2 targetPos, float bulletSpeed, Vector2 position, Vector2 velocity, Vector2 acceleration, Vector2 jerk, float timeToHit)
    {
        Vector2 delta = targetPos - turretPos;
        float angle = Angle.FromVector(delta) - this.angle;
        float distance = delta.Length();
        float t = MathF.Min((distance / bulletSpeed + 0 * angle / turnSpeed), timeToHit);
        return Forecast(position, velocity, acceleration, jerk, t);
    }

    public Vector2 Forecast(Vector2 p, Vector2 v, Vector2 a, Vector2 j, float t)
    {
        return p + v * t + (1 / 2f) * a * t * t + (1 / 6f) * j * t * t * t;
    }

    public override void Serialize(BinaryWriter writer)
    {
        throw new NotImplementedException();
    }
}