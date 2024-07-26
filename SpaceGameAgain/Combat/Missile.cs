using SpaceGame.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class Missile : Actor, IDestructable
{
    public UnitBase Target { get; }
    public Vector2 TargetOffset { get; set; }

    public Vector2 Velocity { get; set; }
    public float Thrust { get; set; }
    public float MaxSpeed { get; set; }
    public bool IsDestroyed { get; set; }

    public Vector2 LastAcceleration { get; set; }
    public Vector2 CurrentAcceleration { get; set; }

    public bool exploding = false;
    private float explosionProgress;
    private float age;

    public Missile(Transform transform, UnitBase target, Vector2 targetOffset, float thrust, float maxSpeed)
    {
        Target = target;
        TargetOffset = targetOffset;
        Transform = transform;
        Thrust = thrust;
        MaxSpeed = maxSpeed;

        Velocity = Angle.ToVector(transform.Rotation) * maxSpeed;
    }

    public override void Update(float tickProgress)
    {
        World.GetSphereOfInfluence(Transform.Position)?.ApplyTo(this);

        if (exploding)
        {
            if (explosionProgress > 1)
            {
                IsDestroyed = true;
            }

            explosionProgress += Time.DeltaTime;
            return;
        }

        if (age > 5)
        {
            IsDestroyed = true;
            return;
        }


        var positionDelta = (Target.Transform.Position + TargetOffset - Transform.Position).Normalized() * MaxSpeed;

        if (Vector2.Distance(Target.Transform.Position + TargetOffset, Transform.Position) < .05f)
        {
            TargetOffset = Vector2.Zero;
        }

        var lastVelocity = Velocity;
        Velocity = Util.Step(Velocity, positionDelta, Thrust * Time.DeltaTime);
        LastAcceleration = CurrentAcceleration;
        CurrentAcceleration = (Velocity - lastVelocity) / Time.DeltaTime;

        Transform.Position += Velocity * Time.DeltaTime;

        if (age > 1 && Target.TestPoint(Transform.Position, Transform.Default))
        {
            Detonate();
            Target.Damage();
        }

        age += Time.DeltaTime;
    }

    public void Detonate()
    {
        Velocity = Vector2.Zero;
        exploding = true;
    }

    public override void Render(ICanvas canvas)
    {
        if (exploding)
        {
            float step = explosionProgress * 9 % 1f;
            if (step < 1 / 4f)
            {
                canvas.Fill(Color.Yellow);
                canvas.DrawCircle(0, 0, .15f);
            }
            else if (step < 2f / 4f)
            {
                canvas.Fill(Color.LightYellow);
                canvas.DrawCircle(0, 0, .15f);
            }

            return;
        }

        canvas.Fill(Color.Lerp(Color.Yellow, Color.LightYellow, 5 * Time.TotalTime % 1f));
        canvas.DrawCircle(0, 0, .01f);
    }
}
