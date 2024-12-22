
using SimulationFramework;
using SimulationFramework.Drawing;
using SpaceGame;
using SpaceGame.Ships;
using SpaceGame.Planets;
//using SpaceGame.Stations;
using System.Numerics;
using SpaceGame.Structures;
using SpaceGame.GUI;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using SimulationFramework.Desktop;
using SimulationFramework.Drawing.Shaders;
using ImGuiNET;

DesktopPlatform.Register();
Start<Program>();

partial class Program : Simulation
{
    ITexture view;
    ITexture uiView;
    public static IFont font;
    public static Vector2 ViewportMousePosition;
    const float UIResolutionScale = 2f;

    public override void OnInitialize()
    {
        Time.MaxDeltaTime = 1 / 30f;
        // view = Graphics.CreateTexture(640, 480);
        font ??= Graphics.LoadFont("Assets/Fonts/VictorMono-Regular.ttf");

        Prototypes.Load();

        World = new();

        var playerTeam = new Team(Prototypes.Get<TeamPrototype>("team"), World.NewID(), Transform.Default);
        var enemies = new Team(Prototypes.Get<TeamPrototype>("team"), World.NewID(), Transform.Default);
        var neutral = new Team(Prototypes.Get<TeamPrototype>("team"), World.NewID(), Transform.Default);

        playerTeam.MakeEnemies(enemies);

        World.Add(playerTeam);
        World.Add(enemies);
        World.Add(neutral);

        World.PlayerTeam = playerTeam.AsReference();
        World.NeutralTeam = neutral;

        World.Add(new Ship((ShipPrototype)Prototypes.Get("ship"), World.NewID(), Transform.Default, ActorReference<Team>.Create(playerTeam)));

        World.Ships[0].modules.Add(new ConstructionModule(World.Ships[0]));

        PlanetPrototype planetProto = Prototypes.Get<PlanetPrototype>("generic_planet");

        var sun = new Planet(planetProto, World.NewID(), Transform.Default)
        {
            Color = Color.Yellow,
            Radius = 50,
        };

        World.Add(sun);
        
        var planet1 = new Planet(planetProto, World.NewID(), Transform.Default)
        {
            Orbit = new(((Actor)sun).AsReference(), 500, 0),
            Color = Color.DarkGreen,
            Radius = 26,
        }; 
        Grid.FillRadius(planet1.Grid, planet1.Radius);
        World.Add(planet1);

        var moon = new Planet(planetProto, World.NewID(), Transform.Default)
        {
            Orbit = new(((Actor)planet1).AsReference(), 100, MathF.PI * 1.25f),
            Color = Color.DarkGray,
            Radius = 9,
        };
        Grid.FillRadius(moon.Grid, moon.Radius);
        World.Add(moon);

        var planet2 = new Planet(planetProto, World.NewID(), Transform.Default)
        {
            Orbit = new(((Actor)sun).AsReference(), 400, MathF.PI * .75f),
            Color = Color.DarkOliveGreen,
            Radius = 17,
        };
        Grid.FillRadius(planet2.Grid, planet2.Radius);
        World.Add(planet2);

        var planet3 = new Planet(planetProto, World.NewID(), Transform.Default)
        {
            Orbit = new(((Actor)sun).AsReference(), 800, MathF.PI * 1.75f),
            Color = Color.Lerp(Color.OrangeRed, Color.Black, 0.25f),
            Radius = 13,
        };
        Grid.FillRadius(planet3.Grid, planet3.Radius);
        World.Add(planet3);

        World.Camera.Transform.Position = planet1.Transform.Position;
        World.Camera.SmoothTransform.Position = planet1.Transform.Position;

        planet1.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("particle_accelerator"), new(0, 0), 0, playerTeam);

        //planet1.Grid.PlaceStructure(World.Structures.ResourceNode, new(0, 0), 0, World.NeutralTeam);

        //planet1.Grid.PlaceStructure(World.Structures.SmallShipyard, new(5, -5), 0, enemies);
        //planet1.Grid.PlaceStructure(World.Structures.ChaingunTurret, new(3, -3), 0, enemies);

        planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("headquarters"), new(0, 0), 1, enemies);
        planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("shipyard"), new(1, 3), 0, enemies);

        planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("missile_turret"), new(2, 0), 0, enemies);
        planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("missile_turret"), new(2, 4), 0, enemies);
        planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("missile_turret"), new(-2, 4), 0, enemies);

        planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("chaingun_turret"), new(3, 1), 0, enemies);
        planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("chaingun_turret"), new(0, 5), 0, enemies);
        planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("chaingun_turret"), new(-1, 2), 0, enemies);

        World.Ships.First().Transform.Position = planet1.Transform.Position;


        // World.LeftSidebar = new Sidebar(Alignment.TopLeft, 300, 400);
        // World.RightSidebar = new Sidebar(Alignment.TopRight, 300, 400);
        // World.RightSidebar.Stack.AddRange([new Label("Hello!", 32) { Alignment = Alignment.CenterRight }, new Label("World!", 16)]);

        // World.LeftSidebar.Stack = new([d
        //     new DynamicLabel(() => $"Materials: {playerTeam.Materials}"),
        // ]);
    }

    bool shouldBeFullscreen;

    public override void OnRender(ICanvas canvas)
    {
        Window.Title = "SpaceGame - " + Performance.Framerate;

        if (Keyboard.IsKeyPressed(Key.F11))
        {
            if (shouldBeFullscreen)
            {
                Window.ExitFullscreen();
            }
            else
            {
                Window.EnterFullscreen();
            }

            shouldBeFullscreen = !shouldBeFullscreen;
        }

        float aspectRatio = canvas.Width / (float)canvas.Height;
        int targetViewWidth = (int)(480 * aspectRatio);

        if (view is null || view.Width != targetViewWidth)
        {
            view?.Dispose();
            uiView?.Dispose();
            view = Graphics.CreateTexture(targetViewWidth, 480);
            uiView = Graphics.CreateTexture((int)(targetViewWidth * UIResolutionScale), (int)(480 * UIResolutionScale));
        }

        float vpScaleY = canvas.Height / (float)view.Height;
        float vpScaleX = canvas.Width / (float)view.Width;

        MatrixBuilder viewMatrix = new MatrixBuilder()
            .Translate(canvas.Width / 2f, canvas.Height / 2f)
            .Scale(MathF.Min(vpScaleX, vpScaleY))
            .Translate(-view.Width / 2f, -view.Height / 2f);

        // if (vpScaleY < vpScaleX)
        // {
        //     float leftGap = canvas.Width / 2f - (view.Width * vpScaleY) / 2f - World.LeftSidebar.MinWidth;
        //     World.LeftSidebar.Width = MathF.Min(World.LeftSidebar.MinWidth + leftGap, World.LeftSidebar.MaxWidth);
        //     
        //     float rightGap = canvas.Width / 2f - (view.Width * vpScaleY) / 2f - World.RightSidebar.MinWidth;
        //     World.RightSidebar.Width = MathF.Min(World.RightSidebar.MinWidth + rightGap, World.RightSidebar.MaxWidth);
        // }
        // else
        // {
        //     World.LeftSidebar.Width = World.LeftSidebar.MinWidth;
        //     World.RightSidebar.Width = World.RightSidebar.MinWidth;
        // }

        DebugMenu.Layout();

        World.Camera.Update(view.Width, view.Height);
        Rectangle vp = new(0, 0, view.Width, view.Height);

        ViewportMousePosition = Vector2.Transform(Mouse.Position, viewMatrix.InverseMatrix);
        World.InfoWindow.CalculateBounds(view.Width, view.Height, out var infoBounds, out _);
        World.MapWindow.CalculateBounds(view.Width, view.Height, out var mapBounds, out _);

        bool worldFocused = vp.ContainsPoint(ViewportMousePosition) && 
            !infoBounds.ContainsPoint(ViewportMousePosition) && 
            !mapBounds.ContainsPoint(ViewportMousePosition);

        World.Update(ViewportMousePosition, worldFocused);
        World.InfoWindow.Update(view.Width, view.Height);
        World.MapWindow.Update(view.Width, view.Height);
        // World.LeftSidebar.Update(canvas.Width, canvas.Height);
        // World.RightSidebar.Update(canvas.Width, canvas.Height);

        if (canvas.Width is 0 && canvas.Height is 0)
            return;

        RenderView();
        RenderUI();

        canvas.Clear(Color.FromHSV(0, 0, .1f));

        canvas.PushState();
        canvas.Transform(viewMatrix.Matrix);
        canvas.DrawTexture(view);
        canvas.Scale(1f / UIResolutionScale);
        canvas.DrawTexture(uiView);
        canvas.PopState();

        // canvas.Font(font);
        // canvas.PushState();
        // World.LeftSidebar.Render(canvas);
        // canvas.PopState();
        // 
        // canvas.PushState();
        // World.RightSidebar.Render(canvas);
        // canvas.PopState();

    }

    private void RenderView()
    {
        var canvas = view.GetCanvas();
        canvas.ResetState();

        canvas.PushState();
        canvas.Antialias(true);
        World.Camera.RenderSetup(canvas);
        World.Render(canvas);
        DebugDraw.Flush(canvas);

        // canvas.ResetState();
        // canvas.Font(font);
        // 
        // canvas.PushState();
        // World.InfoWindow.Render(canvas);
        // canvas.PopState();
        // 
        // canvas.PushState();
        // World.MapWindow.Render(canvas);
        // canvas.PopState();

        canvas.PopState();
    }

    private void RenderUI()
    {
        var canvas = uiView.GetCanvas();
        canvas.ResetState();
        canvas.Font(font);
        canvas.Scale(UIResolutionScale);

        canvas.PushState();
        World.InfoWindow.Render(canvas, (int)(canvas.Width / UIResolutionScale), (int)(canvas.Height / UIResolutionScale));
        canvas.PopState();

        canvas.PushState();
        World.MapWindow.Render(canvas, (int)(canvas.Width / UIResolutionScale), (int)(canvas.Height / UIResolutionScale));
        canvas.PopState();


    }
}
