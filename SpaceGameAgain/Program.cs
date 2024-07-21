
using SimulationFramework;
using SimulationFramework.Drawing;
using SkiaSharp;
using SpaceGame;
using SpaceGame.Ships;
using SpaceGame.Planets;
using SpaceGame.Stations;
using System.Numerics;
using SpaceGame.Structures;
using SpaceGame.GUI;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using SimulationFramework.Desktop;

DesktopPlatform.Register();
Start<Program>();

partial class Program : Simulation
{
    ITexture view;
    public static IFont font;
    public static Vector2 ViewportMousePosition;

    public override void OnInitialize()
    {
        Time.MaxDeltaTime = 1 / 30f;
        view = Graphics.CreateTexture(640, 480);
        font ??= Graphics.LoadFont("Assets/Fonts/VictorMono-Regular.ttf");

        World = new();

        var playerTeam = new Team();
        var enemies = new Team();

        playerTeam.MakeEnemies(enemies);

        World.Teams.AddRange([playerTeam, enemies]);
        World.PlayerTeam = playerTeam;

        World.Camera = new FreeCamera();

        World.Ships.Add(new Ship(playerTeam));

        World.Ships[0].modules.Add(new ConstructionModule(World.Ships[0]));

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

        // planet1.Grid.PlaceStructure(World.Structures.Shipyard, new(5, -5), 0, enemies);
        // planet1.Grid.PlaceStructure(World.Structures.ChaingunTurret, new(3, -3), 0, enemies);
        // 
        // planet3.Grid.PlaceStructure(World.Structures.Headquarters, new(0, 0), 1, enemies);
        // planet3.Grid.PlaceStructure(World.Structures.Shipyard, new(1, 3), 0, enemies);
        // 
        // planet3.Grid.PlaceStructure(World.Structures.MissileTurret, new(2, 0), 0, enemies);
        // planet3.Grid.PlaceStructure(World.Structures.MissileTurret, new(2, 4), 0, enemies);
        // planet3.Grid.PlaceStructure(World.Structures.MissileTurret, new(-1, 4), 0, enemies);
        // 
        // planet3.Grid.PlaceStructure(World.Structures.ChaingunTurret, new(3, 1), 0, enemies);
        // planet3.Grid.PlaceStructure(World.Structures.ChaingunTurret, new(0, 5), 0, enemies);
        // planet3.Grid.PlaceStructure(World.Structures.ChaingunTurret, new(-1, 2), 0, enemies);

        World.Ships.First().Transform.Position = planet1.Transform.Position;

        World.LeftSidebar = new Sidebar(Alignment.TopLeft, 300, 400);
        World.RightSidebar = new Sidebar(Alignment.TopRight, 300, 400);
        World.RightSidebar.Stack.AddRange([new Label("Hello!", 32) { Alignment = Alignment.CenterRight }, new Label("World!", 16)]);

        World.LeftSidebar.Stack = new([
            new DynamicLabel(() => $"Materials: {playerTeam.Materials}"),
        ]);
    }

    public override void OnRender(ICanvas canvas)
    {
        float vpScaleY = canvas.Height / (float)view.Height;
        float vpScaleX = (canvas.Width - (World.LeftSidebar.MinWidth + World.RightSidebar.MinWidth)) / (float)view.Width;

        MatrixBuilder viewMatrix = new MatrixBuilder()
            .Translate(canvas.Width / 2f, canvas.Height / 2f)
            .Scale(MathF.Min(vpScaleX, vpScaleY))
            .Translate(-view.Width / 2f, -view.Height / 2f);

        if (vpScaleY < vpScaleX)
        {
            float leftGap = canvas.Width / 2f - (view.Width * vpScaleY) / 2f - World.LeftSidebar.MinWidth;
            World.LeftSidebar.Width = MathF.Min(World.LeftSidebar.MinWidth + leftGap, World.LeftSidebar.MaxWidth);
            
            float rightGap = canvas.Width / 2f - (view.Width * vpScaleY) / 2f - World.RightSidebar.MinWidth;
            World.RightSidebar.Width = MathF.Min(World.RightSidebar.MinWidth + rightGap, World.RightSidebar.MaxWidth);
        }
        else
        {
            World.LeftSidebar.Width = World.LeftSidebar.MinWidth;
            World.RightSidebar.Width = World.RightSidebar.MinWidth;
        }

        World.Camera.Update(view.Width, view.Height);
        Rectangle vp = new(0, 0, view.Width, view.Height);

        ViewportMousePosition = Vector2.Transform(Mouse.Position, viewMatrix.InverseMatrix);
        World.Update(ViewportMousePosition, vp.ContainsPoint(ViewportMousePosition));
        World.LeftSidebar.Update(canvas.Width, canvas.Height);
        World.RightSidebar.Update(canvas.Width, canvas.Height);

        if (canvas.Width is 0 && canvas.Height is 0)
            return;

        RenderView();

        canvas.Clear(Color.FromHSV(0, 0, .1f));

        canvas.Font(font);
        canvas.PushState();
        World.LeftSidebar.Render(canvas);
        canvas.PopState();
        
        canvas.PushState();
        World.RightSidebar.Render(canvas);
        canvas.PopState();

        canvas.Transform(viewMatrix.Matrix);
        canvas.DrawTexture(view);
    }

    private void RenderView()
    {
        var canvas = view.GetCanvas();

        canvas.PushState();
        canvas.Antialias(false);
        World.Camera.RenderSetup(canvas);
        World.Render(canvas);
        DebugDraw.Flush(canvas);
        canvas.PopState();
    }
}
