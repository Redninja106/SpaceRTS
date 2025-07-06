using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets;
internal class Star : Planet
{
    public Star(PlanetPrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
    }

}

class StarPrototype : PlanetPrototype
{
    public override Type ActorType => typeof(Star);
}