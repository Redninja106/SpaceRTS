using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class ResourceDeposit : Structure
{
    public ResourceDeposit(StructurePrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
    }
}

class ResourceDepositPrototype : StructurePrototype
{
    public override Type ActorType => typeof(ResourceDepositPrototype);
}
