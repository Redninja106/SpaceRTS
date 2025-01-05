using SimulationFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class BulletPrototype() : WorldActorPrototype()
{
    public float Speed { get; set; }

    public override WorldActor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        Transform transform = reader.ReadTransform();
        ActorReference<Missile> target = reader.ReadActorReference<Missile>();
        float lifetime = reader.ReadSingle();

        return new Bullet(this, id, transform, target, lifetime);
    }
}
