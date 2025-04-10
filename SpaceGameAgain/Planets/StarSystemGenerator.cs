using SpaceGame.Structures;
using SpaceGame.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets;
internal class StarSystemGenerator
{
    private PlanetPrototype planetPrototype;
    private Random random;

    public StarSystemGenerator(PlanetPrototype planetPrototype, Random random)
    {
        this.planetPrototype = planetPrototype;
        this.random = random;
    }

    public void GenerateSystem()
    {
        var star = new Planet(planetPrototype, World.NewID(), Transform.Default, null)
        {
            Radius = random.NextSingle(75, 125),
            Color = Color.Yellow,
        };
        World.Add(star);

        float orbitDistance = star.Radius * 3;
        int planetCount = random.Next(12, 12);
        for (int i = 0; i < planetCount; i++)
        {
            float planetRadius = random.NextSingle(5, 30);

            orbitDistance += planetRadius * 10;

            var planet = new Planet(
                planetPrototype,
                World.NewID(),
                Transform.Default,
                new Orbit(
                    star.AsReference<WorldActor>(),
                    orbitDistance,
                    random.NextSingle(0, MathF.PI * orbitDistance)
                )
            )
            { 
                Radius = planetRadius,
                Color = Color.FromHSV(random.NextSingle(), random.NextSingle(), random.NextSingle())
            };

            planet.SphereOfInfluence.Radius = planetRadius * 5;

            Grid.FillRadius(planet.Grid, planetRadius);
            if (random.NextSingle() < 1)
            {
                var cell = planet.Grid.GetCell(new(
                    random.Next((int)(-planetRadius), (int)(planetRadius)), 
                    random.Next((int)(-planetRadius), (int)(planetRadius))
                    ));
                if (cell != null)
                {
                    cell.Tile = new Tile(Prototypes.Get<TilePrototype>("lithium_deposit"));
                }
            }
            World.Add(planet);

            orbitDistance += planetRadius * 10;
        }
        star.SphereOfInfluence.Radius = orbitDistance + star.Radius;
    }
}
