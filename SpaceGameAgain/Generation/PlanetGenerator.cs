using SpaceGame.Data.Converters;
using SpaceGame.Planets;
using SpaceGame.Rendering;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Generation;
internal class PlanetGenerator : DataPrototype
{
    public RandomNumber Size { get; set; } = RandomNumber.Zero;

    public float MinimumSize { get; set; } = 0;
    public float MaximumSize { get; set; } = 0;

    public required PlanetPrototype[] PlanetPrototypes { get; set; }

    public MoonPass[] MoonPasses { get; set; } = [];

    public virtual Planet Generate(GameWorld world, Random random)
    {
        DebugLog.Assert(PlanetPrototypes != null && PlanetPrototypes.Length > 0, $"Prototype {Name ?? "$anonymous"} requires a planet type!");
        
        var prototype = random.GetItems(PlanetPrototypes, 1)[0];
        var planet = prototype.CreateActor(world, world.NewID());
        world.Add(planet);
        if (MaximumSize == 0)
        {
            planet.Radius = MinimumSize;
        }
        else
        {
            planet.Radius = random.NextSingle(MinimumSize, MaximumSize);
        }
        if (!prototype.CanBuild)
        {
            //planet.SphereOfInfluence.Radius = 1500;
            planet.SphereOfInfluence.Radius = planet.Radius * 3;
        }
        else
        {
            planet.SphereOfInfluence.Radius = planet.Radius * 3;
            Grid.FillRadius(planet.Grid, planet.Radius);
        }

        float distance = planet.Radius * 2;
        foreach (var moonPass in MoonPasses)
        {
            moonPass.Generate(world, random, ref distance, planet);
        }

        planet.SphereOfInfluence.Radius = float.Max(planet.SphereOfInfluence.Radius, distance);

        return planet;
    }
}

[DerivedType(typeof(RandomMoonPass), "random")]
[DerivedType(typeof(ConstantMoonPass), "constant")]
[DerivedType(typeof(GapMoonPass), "gap")]
abstract class MoonPass
{
    public abstract void Generate(GameWorld world, Random random, ref float distance, Planet planet);
}

class GapMoonPass : MoonPass
{
    public RandomNumber Size { get; set; } = RandomNumber.Zero;

    public override void Generate(GameWorld world, Random random, ref float distance, Planet planet)
    {
        distance += (float)Size.Get(random);
    }
}

class RandomMoonPass : MoonPass
{
    // public RandomNumber Count { get; set; } = RandomNumber.Zero;
    public int MinimumCount { get; set; } = 0;
    public int MaximumCount { get; set; } = 0;

    // public required PlanetGenerator[] Generators { get; set; }
    public required RandomPrototype<PlanetGenerator> Generators { get; set; }

    public override void Generate(GameWorld world, Random random, ref float distance, Planet planet)
    {
        int count = random.Next(MinimumCount, MaximumCount + 1);
        for (int i = 0; i < count; i++)
        {
            PlanetGenerator moonGenerator = Generators.Get(random);
            Planet moon = moonGenerator.Generate(world, random);
            distance += moon.SphereOfInfluence.Radius;
            moon.orbit = new(planet, distance, random.NextSingle() * MathF.Tau * distance);
            distance += moon.SphereOfInfluence.Radius;
        }
    }
}

class ConstantMoonPass : MoonPass
{
    public required PlanetGenerator[] Generators { get; set; }

    public override void Generate(GameWorld world, Random random, ref float distance, Planet planet)
    {
        foreach (var generator in Generators)
        {
            Planet moon = generator.Generate(world, random);
            moon.orbit = new(planet, distance + moon.SphereOfInfluence.Radius * 2, random.NextSingle() * MathF.Tau * distance);
            distance += moon.SphereOfInfluence.Radius * 2;
        }
    }
}

