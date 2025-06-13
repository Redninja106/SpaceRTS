using SimulationFramework;
using SpaceGame.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class BulletPrototype : Prototype
{
    public override Type ActorType => typeof(Bullet);

    public float Speed { get; set; }
    public float Damage { get; set; } = .01f;
    public int Lifetime { get; set; } = 100;

    //public override Actor Deserialize(BinaryReader reader)
    //{
    //    ulong id = reader.ReadUInt64();
    //    Transform transform = reader.ReadTransform();
    //    ActorReference<Missile> target = reader.ReadActorReference<Missile>();
    //    float lifetime = reader.ReadSingle();

    //    return new Bullet(this, id, transform, target, lifetime);
    //}
}
