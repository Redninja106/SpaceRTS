using SpaceGame.Extensions;
using SpaceGame.Planets;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class Bullet : Actor, IDestructable
{
    public override BulletPrototype Prototype => (BulletPrototype)base.Prototype;

    public required IDamagable target;
    public required int lifetime;
    public SphereOfInfluence? sphereOfInfluence;

    public bool IsDestroyed => lifetime <= 0;

    public Bullet(BulletPrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
        // this.target = target;
    }

    public override void InitializeActor()
    {
        base.InitializeActor();
        this.sphereOfInfluence = World.GetSphereOfInfluence(this.Transform.Position);

    }

    public override void Tick()
    {
        base.Tick();

        sphereOfInfluence?.ApplyTickTo(ref this.Transform);
        Transform.Position += Transform.Forward * Prototype.Speed * Program.Timestep;

        bool hit = target switch
        {
            Missile => Vector2.Distance(Transform.Position.ToVector2(), target.Transform.Position.ToVector2()) < 0.1f,
            Ship s => s.TestPoint(Transform.Position),
        };

        if (hit)
        {
            //DebugDraw.Circle(Vector2.Zero, 0.15f, this.Transform, Color.Orange);
            target.Damage(new DamageInfo() { Amount = Prototype.Damage, Kind = DamageKind.Normal, source = null });
            lifetime = 0;
        }

        lifetime--;
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

    //public override void Serialize(BinaryWriter writer)
    //{
    //    writer.Write(ID);
    //    writer.Write(Transform);
    //    writer.Write(target);
    //    writer.Write(lifetime);
    //}
}
