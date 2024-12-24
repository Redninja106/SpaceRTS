using SpaceGame.Planets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class Bullet : Actor, IDestructable
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

    public override void Update()
    {
        sphereOfInfluence?.ApplyTo(ref this.Transform);
        Transform.Position += Transform.Forward * Prototype.Speed * Time.DeltaTime;

        Rectangle bounds = new(0, 0, .1f, .015f, Alignment.Center);
        if (bounds.ContainsPoint(Transform.WorldToLocal(target.Actor!.Transform.Position)))
        {
            target.Actor!.Detonate();
        }

        // if (Vector2.Distance(Transform.Position, target.Transform.Position) < 0.015f)
        // {
        //     target.Detonate();
        // }

        lifetime -= Time.DeltaTime;
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Fill(Color.Yellow with { A = (byte)MathF.Min(255, 255 * (lifetime - lifetime + .9f * lifetime)) });
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
