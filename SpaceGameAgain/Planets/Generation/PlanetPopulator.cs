using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets.Generation;
internal abstract class PlanetPopulator : DataPrototype
{
    public abstract void Populate(GameWorld world, Random random, Planet planet);
}
