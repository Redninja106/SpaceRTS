using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class MissilePrototype : WorldActorPrototype
{
    public float Acceleration { get; set; }
    public float MaxSpeed { get; set; }

    public override WorldActor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        Transform transform = reader.ReadTransform();
        ActorReference<Unit> target = reader.ReadActorReference<Unit>();
        Vector2 velocity = reader.ReadVector2();
        Vector2 targetOffset = reader.ReadVector2();
        Vector2 lastAcceleration = reader.ReadVector2();
        Vector2 currentAcceleration = reader.ReadVector2();
        bool exploding = reader.ReadBoolean();
        float explosionProgress = reader.ReadSingle();
        float age = reader.ReadSingle();
        
        return new Missile(this, id, transform, target, targetOffset)
        {
            Velocity = velocity,
            LastAcceleration = lastAcceleration,
            CurrentAcceleration = currentAcceleration,
            exploding = exploding,
            explosionProgress = explosionProgress,
            age = age,
        };
    }
}
