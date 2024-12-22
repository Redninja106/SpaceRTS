using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class BulletPrototype() : Prototype()
{
    public override Type ActorType => typeof(Bullet);

    public override Actor? Deserialize(BinaryReader reader)
    {
        throw new NotImplementedException();
    }
}
