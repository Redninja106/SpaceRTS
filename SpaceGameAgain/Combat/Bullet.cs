using SpaceGame.Planets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class Bullet : WorldActor, IDestructable
{
    public override BulletPrototype Prototype => (BulletPrototype)base.Prototype;

    private ActorReference<Missile> target;
    private float lifetime;
    public SphereOfInfluence? sphereOfInfluence;

    public bool IsDestroyed => lifetime <= 0;

    public Bullet(BulletPrototype prototype, ulong id, Transform transform, ActorReference<Missile> target, float lifetime) : base(prototype, id, transform)
    {
        Transform = transform;
        this.target = target;
        this.lifetime = lifetime;
        this.sphereOfInfluence = World.GetSphereOfInfluence(transform.Position);
    }

    public override void Tick()
    {
        base.Tick();

        sphereOfInfluence?.ApplyTo(ref this.Transform);
        Transform.Position += Transform.Forward * Prototype.Speed * Program.Timestep;
        
        if (Vector2.Distance(Transform.Position, target.Actor!.Transform.Position) < 0.1f)
        {
            //DebugDraw.Circle(Vector2.Zero, 0.15f, this.Transform, Color.Orange);
            target.Actor!.Detonate();
        }
        else
        {
            //DebugDraw.Circle(Vector2.Zero, 0.15f, this.Transform, Color.Blue);
        }

        lifetime -= Program.Timestep;
    }

    public override void Render(ICanvas canvas)
    {
        // TODO: add alpha effect
        canvas.Fill(Color.Yellow);
        canvas.DrawRect(0, 0, .1f, .015f, Alignment.Center);
    }

    public void OnDestroyed()
    {
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);
        writer.Write(Transform);
        writer.Write(target);
        writer.Write(lifetime);
    }
}
