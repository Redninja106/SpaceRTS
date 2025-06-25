using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Data;
internal class DataPrototype : Prototype
{
    public sealed override Type ActorType => throw new NotSupportedException($"Data Prototype {GetType().Name} does not have an actor type");

    public sealed override Actor CreateActor(GameWorld world, ulong id)
    {
        throw new NotSupportedException($"Data Prototype {GetType().Name} cannot create actors");
    }
}
