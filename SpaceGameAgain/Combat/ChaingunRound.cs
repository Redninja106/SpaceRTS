using SpaceGame.Planets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class ChaingunRound : Actor, IDestructable
{
    private readonly Missile target;
    private readonly float speed;
    private readonly float lifetime;
    private float age;
    public SphereOfInfluence? sphereOfInfluence;

    public bool IsDestroyed => age > lifetime;

    public ChaingunRound(Transform transform, Missile target, SphereOfInfluence? sphereOfInfluence, float speed, float lifetime)
    {
        Transform = transform;
        this.target = target;
        this.speed = speed;
        this.lifetime = lifetime;
        this.sphereOfInfluence = sphereOfInfluence;
    }

    public override void Update()
    {
        sphereOfInfluence?.ApplyTo(ref this.Transform);
        Transform.Position += Transform.Forward * speed * Time.DeltaTime;

        Rectangle bounds = new(0, 0, .1f, .015f, Alignment.Center);
        if (bounds.ContainsPoint(Transform.WorldToLocal(target.Transform.Position)))
        {
            target.Detonate();
        }

        // if (Vector2.Distance(Transform.Position, target.Transform.Position) < 0.015f)
        // {
        //     target.Detonate();
        // }

        age += Time.DeltaTime;
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Fill(Color.Yellow with { A = (byte)MathF.Min(255, 255 * (lifetime - age + .9f * lifetime)) });
        canvas.DrawRect(0, 0, .1f, .015f, Alignment.Center);
    }
}
