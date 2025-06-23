using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
[Serializable]
internal class Spaceport : Structure
{
    public Spaceport(StructurePrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
    }
}
