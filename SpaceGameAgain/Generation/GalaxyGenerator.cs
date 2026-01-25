using SpaceGame.Planets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Generation;
internal class GalaxyGenerator
{
    PlanetGenerator galacticCoreGenerator = Prototypes.Get<PlanetGenerator>("galactic_core_generator");
    PlanetGenerator innerRingGenerator = Prototypes.Get<PlanetGenerator>("inner_ring_system_generator");
    PlanetGenerator outerRingGenerator = Prototypes.Get<PlanetGenerator>("outer_ring_system_generator");
    PlanetGenerator playerSystemGenerator = Prototypes.Get<PlanetGenerator>("player_system_generator");

    public GalaxyGenerator()
    {

    }

    public void Generate(GameWorld world, Random random)
    {
        Planet center = galacticCoreGenerator.Generate(world, random);

        GenerateRing(world, random, center, innerRingGenerator, 5, 7500);
        GenerateRing(world, random, center, outerRingGenerator, 8, 15000);
        GenerateRing(world, random, center, playerSystemGenerator, 12, 22500);

        center.SphereOfInfluence.Radius = 25000;
    }

    private void GenerateRing(GameWorld world, Random random, Planet center, PlanetGenerator generator, int count, float distance)
    {
        float phaseOffset = random.NextSingle();
        for (int i = 0; i < count; i++)
        {
            var system = generator.Generate(world, random);
            system.orbit = new(center, distance, (i / (float)count + phaseOffset) * float.Tau);
            system.orbit.speed = world.backgroundShader.Galaxy.turnSpeed * distance;
        }
    }
}
