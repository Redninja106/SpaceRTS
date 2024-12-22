using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class MissilePrototype : Prototype
{
    public override Type ActorType => typeof(Missile);

    public float Acceleration { get; set; }
    public float MaxSpeed { get; set; }

    public override Actor? Deserialize(BinaryReader reader)
    {
        return new Missile(
            this, 
            reader.ReadUInt64(), 
            reader.ReadTransform(),
            reader.ReadActorReference<Unit>(), 
            reader.ReadVector2()
            );
    }
}
