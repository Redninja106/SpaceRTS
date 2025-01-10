using SpaceGame.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class Missile : WorldActor, IDestructable
{
    public override MissilePrototype Prototype => (MissilePrototype)base.Prototype;

    public ActorReference<Unit> Target { get; }
    public DoubleVector TargetOffset { get; set; }

    public DoubleVector Velocity { get; set; }
    public bool IsDestroyed => explosionProgress > 1;

    public DoubleVector LastAcceleration { get; set; }
    public DoubleVector CurrentAcceleration { get; set; }

    public bool exploding = false;
    public float explosionProgress;
    public float age;

    public Missile(MissilePrototype prototype, ulong id, Transform transform, ActorReference<Unit> target, DoubleVector targetOffset) : base(prototype, id, transform)
    {
        Target = target;
        TargetOffset = targetOffset;
        Transform = transform;

        Velocity = DoubleVector.FromVector2(Angle.ToVector(transform.Rotation) * prototype.MaxSpeed);
    }

    public override void Tick()
    {
        base.Tick();

        World.GetSphereOfInfluence(Transform.Position)?.ApplyTickTo(this);

        if (exploding)
        {
            explosionProgress += Program.Timestep;
            return;
        }

        if (age > 5)
        {
            exploding = true;
            return;
        }

        var positionDelta = (Target.Actor!.Transform.Position + TargetOffset - Transform.Position).Normalized() * Prototype.MaxSpeed;
        if (DoubleVector.Distance(Target.Actor!.Transform.Position + TargetOffset, Transform.Position) < .05f)
        {
            TargetOffset = DoubleVector.Zero;
        }

        var lastVelocity = Velocity;
        Velocity = Util.Step(Velocity, positionDelta, Prototype.Acceleration * Program.Timestep);
        CurrentAcceleration = (Velocity - lastVelocity) / Program.Timestep;

        Transform.Position += Velocity * Program.Timestep;

        if (age > 1 && Target.Actor!.TestPoint(Transform.Position.ToVector2(), Transform.Default))
        {
            Detonate();
            Target.Actor!.Health--;
        }

        age += Program.Timestep;
    }

    public void Detonate()
    {
        Velocity = DoubleVector.Zero;
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

    public void OnDestroyed()
    {
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);
        writer.Write(Transform);
        writer.Write(Target);
        writer.Write(TargetOffset);
        writer.Write(Velocity);
        writer.Write(LastAcceleration);
        writer.Write(CurrentAcceleration);
        writer.Write(exploding);
        writer.Write(explosionProgress);
        writer.Write(age);
    }
}
