using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets;
internal class DesertPlanet : Planet
{
    public DesertPlanet(PlanetPrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
    }

    public override void Render(ICanvas canvas)
    {
        base.Render(canvas);
    }
}

class DesertPlanetPrototype : PlanetPrototype
{
    public override Type ActorType => typeof(DesertPlanet);
}
