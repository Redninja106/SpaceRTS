using SimulationFramework.Desktop;
using SpaceGame;
using SpaceGame.Commands;
using SpaceGame.Networking;
using SpaceGame.Planets;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
//using SpaceGame.Stations;
using SpaceGame.Structures;
using SpaceGame.Teams;

DesktopPlatform.Register();
Start<Program>();

partial class Program : Simulation
{
    ITexture view;
    public static IFont font;
    public static Vector2 ViewportMousePosition;
    public static float uiscale = 2f;
    public static float uiscaleResolutionFactor = 1 / 1920f;

    public static float actualGUIScale = 0;

    public const float Timestep = 1 / 50f;
    public static float GameSpeed = 1f;
    public static bool forceTickThisFrame = false;

    private float timeAccumulated = 0;

    public static Lobby? Lobby;
    public static NetworkSerializer NetworkSerializer = new();


    public override void OnInitialize()
    {
        Time.MaxDeltaTime = 1 / 30f;
        // view = Graphics.CreateTexture(640, 480);
        font ??= Graphics.LoadFont("Assets/Fonts/VictorMono-Regular.ttf");
        
        Prototypes.Load();

        World = new();

        var playerTeam = new Team(Prototypes.Get<TeamPrototype>("team"), World.NewID(), Transform.Default);
        playerTeam.CommandProcessor = new PlayerCommandProcessor();

        var enemies = new Team(Prototypes.Get<TeamPrototype>("team"), World.NewID(), Transform.Default);
        enemies.CommandProcessor = new PlayerCommandProcessor();


        playerTeam.MakeEnemies(enemies);

        World.Add(playerTeam);
        World.Add(enemies);

        World.PlayerTeam = playerTeam.AsReference();

        World.Add(new Ship((ShipPrototype)Prototypes.Get("ship"), World.NewID(), Transform.Default, ActorReference<Team>.Create(playerTeam)));
        var constMod = new ConstructionModule(Prototypes.Get<ConstructionModulePrototype>("construction_module"), World.NewID(), World.Ships[0].AsReference());
        World.Ships[0].modules.Add(((Module)constMod).AsReference());
        World.Add(constMod);

        World.Add(new Ship((ShipPrototype)Prototypes.Get("ship"), World.NewID(), Transform.Default, ActorReference<Team>.Create(enemies)));
        var constMod2 = new ConstructionModule(Prototypes.Get<ConstructionModulePrototype>("construction_module"), World.NewID(), World.Ships[1].AsReference());
        World.Ships[1].modules.Add(((Module)constMod2).AsReference());
        World.Add(constMod2);

        PlanetPrototype planetProto = Prototypes.Get<PlanetPrototype>("generic_planet");

        var sun = new Planet(planetProto, World.NewID(), Transform.Default)
        {
            Color = Color.Yellow,
            Radius = 50,
        };

        World.Add(sun);
        
        var planet1 = new Planet(planetProto, World.NewID(), Transform.Default)
        {
            Orbit = new(((WorldActor)sun).AsReference(), 500, 0),
            Color = Color.DarkGreen,
            Radius = 26,
        }; 
        Grid.FillRadius(planet1.Grid, planet1.Radius);
        World.Add(planet1);

        var moon = new Planet(planetProto, World.NewID(), Transform.Default)
        {
            Orbit = new(((WorldActor)planet1).AsReference(), 100, MathF.PI * 1.25f),
            Color = Color.DarkGray,
            Radius = 9,
        };
        Grid.FillRadius(moon.Grid, moon.Radius);
        World.Add(moon);

        var planet2 = new Planet(planetProto, World.NewID(), Transform.Default)
        {
            Orbit = new(((WorldActor)sun).AsReference(), 400, MathF.PI * .75f),
            Color = Color.DarkOliveGreen,
            Radius = 17,
        };
        Grid.FillRadius(planet2.Grid, planet2.Radius);
        World.Add(planet2);

        var planet3 = new Planet(planetProto, World.NewID(), Transform.Default)
        {
            Orbit = new(((WorldActor)sun).AsReference(), 800, MathF.PI * 1.75f),
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
            view = Graphics.CreateTexture(targetViewWidth, 480);
        }

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

        float vpScaleY = canvas.Height / (float)view.Height;
        float vpScaleX = canvas.Width / (float)view.Width;

        MatrixBuilder viewMatrix = new MatrixBuilder()
            .Translate(canvas.Width / 2f, canvas.Height / 2f)
            .Scale(MathF.Min(vpScaleX, vpScaleY))
            .Translate(-view.Width / 2f, -view.Height / 2f);

        Update(canvas, viewMatrix);

        if (canvas.Width is 0 && canvas.Height is 0)
            return;

        RenderView();

        canvas.Clear(Color.FromHSV(0, 0, .1f));

        canvas.PushState();
        canvas.Transform(viewMatrix.Matrix);
        canvas.DrawTexture(view);
        canvas.PopState();
        RenderUI(canvas);

        // canvas.Font(font);
        // canvas.PushState();
        // World.LeftSidebar.Render(canvas);
        // canvas.PopState();
        // 
        // canvas.PushState();
        // World.RightSidebar.Render(canvas);
        // canvas.PopState();

    }

    private void Update(ICanvas canvas, MatrixBuilder viewMatrix)
    {
        Lobby?.Update();

        World.Camera.Update(view.Width, view.Height);
        Rectangle vp = new(0, 0, view.Width, view.Height);

        actualGUIScale = uiscale * Math.Clamp(canvas.Width * uiscaleResolutionFactor, 1 / uiscale, 1);

        ViewportMousePosition = Vector2.Transform(Mouse.Position, viewMatrix.InverseMatrix);
        World.InfoWindow.CalculateBounds(canvas.Width / actualGUIScale, canvas.Height / actualGUIScale, out var infoBounds, out _);
        World.MapWindow.CalculateBounds(canvas.Width / actualGUIScale, canvas.Height / actualGUIScale, out var mapBounds, out _);

        bool worldFocused = vp.ContainsPoint(ViewportMousePosition) &&
            !infoBounds.ContainsPoint(Mouse.Position / actualGUIScale) &&
            !mapBounds.ContainsPoint(Mouse.Position / actualGUIScale);

        timeAccumulated += Time.DeltaTime;
        if (forceTickThisFrame || GameSpeed >= 0 && timeAccumulated >= Timestep / GameSpeed)
        {
            forceTickThisFrame = false;
            DebugDraw.Clear();

            World.Tick(ViewportMousePosition, worldFocused);
            timeAccumulated = 0;
        }
        
        float tickProgress = MathF.Min(timeAccumulated * GameSpeed / Timestep, 1);

        if (GameSpeed == 0)
        {
            tickProgress = 1;
        }

        World.Update(ViewportMousePosition, worldFocused, tickProgress);
        World.InfoWindow.Update(canvas.Width / actualGUIScale, canvas.Height / actualGUIScale);
        World.MapWindow.Update(canvas.Width / actualGUIScale, canvas.Height / actualGUIScale);
    }

    private void RenderView()
    {
        var canvas = view.GetCanvas();
        canvas.ResetState();

        canvas.PushState();
        canvas.Antialias(true);
        World.Camera.RenderSetup(canvas);
        World.Render(canvas);
        DebugDraw.Draw(canvas);

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

    private void RenderUI(ICanvas canvas)
    {
        canvas.ResetState();
        canvas.Font(font);

        canvas.Scale(actualGUIScale);

        canvas.PushState();
        World.InfoWindow.Render(canvas, (int)(canvas.Width / actualGUIScale), (int)(canvas.Height / actualGUIScale));
        canvas.PopState();

        canvas.PushState();
        World.MapWindow.Render(canvas, (int)(canvas.Width / actualGUIScale), (int)(canvas.Height / actualGUIScale));
        canvas.PopState();
    }
}
