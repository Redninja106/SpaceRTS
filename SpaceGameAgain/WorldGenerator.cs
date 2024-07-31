using SpaceGame.Combat;
using SpaceGame.GUI;
using SpaceGame.Planets;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Structures;
using SpaceGame.Teams;

namespace SpaceGame;

class WorldGenerator : WorldProvider
{
    public override void CreateActors()
    {
        var playerTeam = new Team();
        var enemies = new Team();

        playerTeam.MakeEnemies(enemies);

        World.Teams.AddRange([playerTeam, enemies]);
        World.PlayerTeam = playerTeam;

        World.Camera = new FreeCamera();

        World.Ships.Add(new Ship(playerTeam));

        World.Ships.First().modules.Add(new ConstructionModule(World.Ships.First()));
        World.Ships.First().modules.Add(new MissileModule(World.Ships.First()));

        var sun = new Planet()
        {
            Color = Color.Yellow,
            Radius = 50,
        };
        World.Planets.Add(sun);

        var planet1 = new Planet()
        {
            Orbit = new(sun, 500, 0, 0),
            Color = Color.DarkGreen,
            Radius = 26,
        };
        Grid.FillRadius(planet1.Grid, planet1.Radius);
        World.Planets.Add(planet1);

        var moon = new Planet()
        {
            Orbit = new(planet1, 100, MathF.PI * 1.25f, 0),
            Color = Color.DarkGray,
            Radius = 9,
        };
        Grid.FillRadius(moon.Grid, moon.Radius);
        World.Planets.Add(moon);

        var planet2 = new Planet()
        {
            Orbit = new(sun, 400, MathF.PI * .75f, 0),
            Color = Color.DarkOliveGreen,
            Radius = 17,
        };
        Grid.FillRadius(planet2.Grid, planet2.Radius);
        World.Planets.Add(planet2);

        var planet3 = new Planet()
        {
            Orbit = new(sun, 800, MathF.PI * 1.75f, 0),
            Color = Color.Lerp(Color.OrangeRed, Color.Black, 0.25f),
            Radius = 13,
        };
        Grid.FillRadius(planet3.Grid, planet3.Radius);
        World.Planets.Add(planet3);

        World.Camera.Transform.Position = planet1.Transform.Position;
        World.Camera.SmoothTransform.Position = planet1.Transform.Position;

        planet1.Grid.PlaceStructure(StructureList.Shipyard, new(5, -5), 0, enemies);
        planet1.Grid.PlaceStructure(StructureList.ChaingunTurret, new(3, -3), 0, enemies);

        planet3.Grid.PlaceStructure(StructureList.Headquarters, new(0, 0), 1, enemies);
        planet3.Grid.PlaceStructure(StructureList.Shipyard, new(1, 3), 0, enemies);

        planet3.Grid.PlaceStructure(StructureList.MissileTurret, new(2, 0), 0, enemies);
        planet3.Grid.PlaceStructure(StructureList.MissileTurret, new(2, 4), 0, enemies);
        planet3.Grid.PlaceStructure(StructureList.MissileTurret, new(-1, 4), 0, enemies);

        planet3.Grid.PlaceStructure(StructureList.ChaingunTurret, new(3, 1), 0, enemies);
        planet3.Grid.PlaceStructure(StructureList.ChaingunTurret, new(0, 5), 0, enemies);
        planet3.Grid.PlaceStructure(StructureList.ChaingunTurret, new(-1, 2), 0, enemies);

        World.Ships.First().Transform.Position = planet1.Transform.Position;

        World.LeftSidebar = new Sidebar(Alignment.TopLeft, 300, 400);
        World.RightSidebar = new Sidebar(Alignment.TopRight, 300, 400);
        World.RightSidebar.Stack.AddRange([new Label("Hello!", 32) { Alignment = Alignment.CenterRight }, new Label("World!", 16)]);

        World.LeftSidebar.Stack = new([
            new DynamicLabel(() => $"Materials: {playerTeam.Materials}"),
        ]);
    }
}
