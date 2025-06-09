using SimulationFramework.Desktop;
using SimulationFramework.Drawing.Shaders;
using SpaceGame;
using SpaceGame.Bots;
using SpaceGame.Commands;
using SpaceGame.GUI;
using SpaceGame.Interaction;
using SpaceGame.Networking;
using SpaceGame.Planets;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
//using SpaceGame.Stations;
using SpaceGame.Structures;
using SpaceGame.Structures.Shipyards;
using SpaceGame.Teams;
using SpaceGame.Tiles;
using System.Diagnostics;

DesktopPlatform.Register();

if (Path.GetFullPath(Environment.CurrentDirectory) != Path.GetDirectoryName(Environment.ProcessPath))
{
    Environment.CurrentDirectory = Path.GetDirectoryName(Environment.ProcessPath)!;
}

DebugLog.Initialize();

if (Debugger.IsAttached)
{
    try
    {
        Start<Program>();
    }
#if RELEASE
catch (Exception ex)
{
    DebugLog.ReportException(ex);
}
#endif
    finally
    {
        DebugLog.Uninitialize();
    }
}
else
{
    try
    {
        Start<Program>();
    }
    catch (Exception ex)
    {
        DebugLog.ReportException(ex);
    }
    finally
    {
        DebugLog.Uninitialize();
    }
}

partial class Program : Simulation
{
    
    public static IFont font;
    public static Vector2 ViewportMousePosition;
    public static float uiscale = 2f;
    public static float uiscaleResolutionFactor = 1 / 1920f;

    public static float actualGUIScale = 0;

    public const float TickRate = 50f;
    public const float Timestep = 1 / TickRate;
    public static float GameSpeed = 1f;
    public static bool forceTickThisFrame = false;

    public static float timeAccumulated = 0;

    public static Lobby? Lobby;
    public static NetworkSerializer NetworkSerializer = new();

    public static float ViewportScale;

    private ITexture viewTexture;
    private ITexture groundTexture;
    private ITexture skyTexture;
    private ITexture visibilityTexture;
    private CompositingShader compositingShader = new CompositingShader();

    public override void OnInitialize()
    {
        Time.MaxDeltaTime = 1 / 30f;
        // view = Graphics.CreateTexture(640, 480);
        font ??= Graphics.LoadFont("Assets/Fonts/VictorMono-Regular.ttf");

        DebugOverlays.Register();
        Prototypes.Load();

        World = new();
        
        var playerTeam = new Team(Prototypes.Get<PlayerTeamPrototype>("player_team"), World.NewID(), Transform.Default);
        playerTeam.Money += 1000;
        // playerTeam.CommandProcessor = new PlayerCommandProcessor();
        World.PlayerTeam = playerTeam.AsReference();
        World.Add(playerTeam);

        var starterShip = new Ship(Prototypes.Get<ShipPrototype>("small_ship"), World.NewID(), Transform.Default, playerTeam.AsReference());
        var module = new ConstructionModule(Prototypes.Get<ConstructionModulePrototype>("construction_module"), World.NewID(), starterShip.AsReference());
        starterShip.modules.Add(module.AsReference<Module>());
        World.Add(starterShip);
        World.Add(module);

        var spacePirates = new Team(Prototypes.Get<TeamPrototype>("null_team"), World.NewID(), Transform.Default, name: "Space Pirates");
        // spacePirates.CommandProcessor = new NullCommandProcessor();
        World.Add(spacePirates);

        // var enemies = new Team(Prototypes.Get<TeamPrototype>("team"), World.NewID(), Transform.Default);
        // enemies.CommandProcessor = new PlayerCommandProcessor();

        // playerTeam.MakeEnemies(enemies);
        // World.Add(playerTeam);
        // World.Add(enemies);

        // World.Add(new Ship((ShipPrototype)Prototypes.Get("small_ship"), World.NewID(), Transform.Default, ActorReference<Team>.Create(playerTeam)));
        // var constMod = new ConstructionModule(Prototypes.Get<ConstructionModulePrototype>("construction_module"), World.NewID(), World.Ships[0].AsReference());
        // World.Ships[0].modules.Add(((Module)constMod).AsReference());
        // World.Add(constMod);

        // World.Add(new Ship((ShipPrototype)Prototypes.Get("small_ship"), World.NewID(), Transform.Default, ActorReference<Team>.Create(enemies)));
        // var constMod2 = new ConstructionModule(Prototypes.Get<ConstructionModulePrototype>("construction_module"), World.NewID(), World.Ships[1].AsReference());
        // World.Ships[1].modules.Add(((Module)constMod2).AsReference());
        // World.Add(constMod2);

        PlanetPrototype planetProto = Prototypes.Get<PlanetPrototype>("star");
        StarSystemGenerator generator = new(planetProto, Random.Shared);
        generator.GenerateSystem();

        Planet starterPlanet = World.Planets[Random.Shared.Next(1, World.Planets.Count)];
        starterShip.Teleport(starterPlanet.Transform);
        World.Camera.Transform = World.Camera.SmoothTransform = starterPlanet.Transform;

        // Planet tradeUnionPlanet = World.Planets[Random.Shared.Next(1, World.Planets.Count)];
        // Team tradeUnion = new Team(Prototypes.Get<TeamPrototype>("team"), World.NewID(), Transform.Default, money: 1000);
        // tradeUnionPlanet.Grid.PlaceStructure(Prototypes.Get<AssemblyBayPrototype>("small_assembly_bay"), HexCoordinate.Zero, 0, tradeUnion);
        // tradeUnionPlanet.Grid.PlaceStructure(Prototypes.Get<ManufactoryPrototype>("manufactory"), HexCoordinate.UnitQ, 0, tradeUnion);
        // tradeUnionPlanet.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("generator"), new HexCoordinate(0, 2), 0, tradeUnion);

        // var sun = new Planet(planetProto, World.NewID(), Transform.Default, null)
        // {
        //     Color = Color.Yellow,
        //     Radius = 50,
        // };
        // sun.SphereOfInfluence.Radius = 1000;
        // 
        // World.Add(sun);
        // 
        // var planet1 = new Planet(planetProto, World.NewID(), Transform.Default, new(((WorldActor)sun).AsReference(), 500, 0))
        // {
        //     Color = Color.DarkGreen,
        //     Radius = 26,
        // };
        // planet1.SphereOfInfluence.Radius = 80;
        // Grid.FillRadius(planet1.Grid, planet1.Radius);
        // World.Add(planet1);
        // 
        // var moon = new Planet(planetProto, World.NewID(), Transform.Default, new(((WorldActor)planet1).AsReference(), 100, MathF.PI * 1.25f))
        // {
        //     Color = Color.DarkGray,
        //     Radius = 9,
        // };
        // Grid.FillRadius(moon.Grid, moon.Radius);
        // World.Add(moon);
        // 
        // var planet2 = new Planet(planetProto, World.NewID(), Transform.Default, new(((WorldActor)sun).AsReference(), 400, MathF.PI * .75f))
        // {
        //     Color = Color.DarkOliveGreen,
        //     Radius = 17,
        // };
        // Grid.FillRadius(planet2.Grid, planet2.Radius);
        // World.Add(planet2);
        // 
        // var planet3 = new Planet(planetProto, World.NewID(), Transform.Default, new(((WorldActor)sun).AsReference(), 800, MathF.PI * 1.75f))
        // {
        //     Color = Color.Lerp(Color.OrangeRed, Color.Black, 0.25f),
        //     Radius = 13,
        // };
        // Grid.FillRadius(planet3.Grid, planet3.Radius);
        // World.Add(planet3);

        //World.Camera.Transform.Position = planet1.Transform.Position;
        //World.Camera.SmoothTransform.Position = planet1.Transform.Position;

        //planet1.Grid.GetCell(HexCoordinate.Zero)!.Tile = new ResourceDepositTile(Prototypes.Get<TilePrototype>("rare_metals_deposit"), World.NewID(), Transform.Default, 100);

        //// planet1.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("particle_accelerator"), new(0, 0), 0, playerTeam);

        ////planet1.Grid.PlaceStructure(World.Structures.ResourceNode, new(0, 0), 0, World.NeutralTeam);

        ////planet1.Grid.PlaceStructure(World.Structures.SmallShipyard, new(5, -5), 0, enemies);
        ////planet1.Grid.PlaceStructure(World.Structures.ChaingunTurret, new(3, -3), 0, enemies);

        //planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("headquarters"), new(0, 0), 1, enemies);

        //planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("missile_turret"), new(2, 0), 0, enemies);
        //planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("missile_turret"), new(2, 4), 0, enemies);
        //planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("missile_turret"), new(-2, 4), 0, enemies);

        //planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("chaingun_turret"), new(3, 1), 0, enemies);
        //planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("chaingun_turret"), new(0, 5), 0, enemies);
        //planet3.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("chaingun_turret"), new(-1, 2), 0, enemies);

        //// planet1.Grid.PlaceStructure(Prototypes.Get<StructurePrototype>("rare_metals_deposit"), new(0, 0), 0, null);

        //World.Ships.First().Transform.Position = planet1.Transform.Position;

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

        if (viewTexture is null || viewTexture.Width != targetViewWidth)
        {
            viewTexture?.Dispose();
            viewTexture = Graphics.CreateTexture(targetViewWidth, 480);

            visibilityTexture?.Dispose();
            visibilityTexture = Graphics.CreateTexture(targetViewWidth, 480);

            groundTexture?.Dispose();
            groundTexture = Graphics.CreateTexture(targetViewWidth, 480);

            skyTexture?.Dispose();
            skyTexture = Graphics.CreateTexture(targetViewWidth, 480);
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

        float vpScaleY = canvas.Height / (float)viewTexture.Height;
        float vpScaleX = canvas.Width / (float)viewTexture.Width;
        ViewportScale = MathF.Min(vpScaleX, vpScaleY);

        MatrixBuilder viewMatrix = new MatrixBuilder()
            .Translate(canvas.Width / 2f, canvas.Height / 2f)
            .Scale(ViewportScale)
            .Translate(-viewTexture.Width / 2f, -viewTexture.Height / 2f);

        Update(canvas, viewMatrix);

        if (canvas.Width is 0 && canvas.Height is 0)
            return;

        RenderViewTexture();

        canvas.Clear(Color.FromHSV(0, 0, .1f));

        canvas.PushState();
        canvas.Transform(viewMatrix.Matrix);
        //canvas.DrawTexture(background);
        //canvas.DrawTexture(foreground);
        canvas.DrawTexture(this.viewTexture);
        canvas.PopState();

        World.GUIViewport.Render(canvas);

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

        timeAccumulated += Time.DeltaTime;
        float tickProgress = MathF.Min(timeAccumulated * GameSpeed / Timestep, 1);
        if (GameSpeed == 0)
        {
            tickProgress = 1;
        }

        World.Camera.Update(viewTexture.Width, viewTexture.Height, tickProgress);
        Rectangle vp = new(0, 0, viewTexture.Width, viewTexture.Height);

        actualGUIScale = uiscale * Math.Clamp(canvas.Width * uiscaleResolutionFactor, 1 / uiscale, 1);

        ViewportMousePosition = Vector2.Transform(Mouse.Position, viewMatrix.InverseMatrix);
        // World.InfoWindow.CalculateBounds(canvas.Width / actualGUIScale, canvas.Height / actualGUIScale, out var infoBounds, out _);
        // World.MapWindow.CalculateBounds(canvas.Width / actualGUIScale, canvas.Height / actualGUIScale, out var mapBounds, out _);

        // bool worldFocused = vp.ContainsPoint(ViewportMousePosition) &&
        //     !infoBounds.ContainsPoint(Mouse.Position / actualGUIScale) &&
        //     !mapBounds.ContainsPoint(Mouse.Position / actualGUIScale);

        if (forceTickThisFrame || GameSpeed >= 0 && timeAccumulated >= Timestep / GameSpeed)
        {
            forceTickThisFrame = false;
            DebugDraw.Clear();

            DebugMenu.ClearMetrics();
            World.Tick(ViewportMousePosition);

            timeAccumulated = 0;
            tickProgress = 0;
        }

        World.GUIViewport.Update(canvas.Width, canvas.Height);
        World.Update(ViewportMousePosition, tickProgress);
    }

    private void RenderViewTexture()
    {
        var canvas = viewTexture.GetCanvas();
        canvas.ResetState();
        canvas.Clear(Color.Black);

        var visibilityCanvas = visibilityTexture.GetCanvas();
        visibilityCanvas.ResetState();
        visibilityCanvas.Clear(Color.Transparent);

        var groundCanvas = groundTexture.GetCanvas();
        groundCanvas.ResetState();
        groundCanvas.Clear(Color.Transparent);

        var skyCanvas = skyTexture.GetCanvas();
        skyCanvas.ResetState();
        skyCanvas.Clear(Color.Transparent);

        canvas.PushState();
        canvas.Antialias(true);
        World.Camera.RenderSetup(canvas);

        // prep visibility texture for compositing the layers
        World.RenderVisibility(visibilityCanvas);
        visibilityCanvas.Flush();

        // BACKGROUND LAYER
        World.RenderBackgroundLayer(canvas);
        World.RenderBackgroundOverlayLayer(canvas);

        // GROUND LAYER
        World.RenderGroundLayer(groundCanvas);
        groundCanvas.Flush();
        compositingShader.Composite(canvas, groundTexture, visibilityTexture, false);
        World.RenderGroundOverlayLayer(canvas);

        // SKY LAYER
        World.RenderSkyLayer(skyCanvas);
        skyCanvas.Flush();
        compositingShader.Composite(canvas, skyTexture, visibilityTexture, true);
        World.RenderSkyOverlayLayer(canvas);

        //canvas.PushState();
        //canvas.ResetState();
        //groundCanvas.Flush();
        //visibilityCanvas.Flush();
        //compositingShader.foregroundTexture = this.groundTexture;
        //compositingShader.visibiltyTexture = this.visibilityTexture;
        //compositingShader.Time = Time.TotalTime;
        //canvas.Fill(compositingShader);
        //canvas.DrawRect(0, 0, canvas.Width, canvas.Height);
        //canvas.PopState();


        // var backgroundCanvas = background.GetCanvas();
        // backgroundCanvas.ResetState();
        // backgroundCanvas.Clear(Color.Black);
        // World.RenderBackgroundLayer(backgroundCanvas);
        // 
        // var foregroundCanvas = foreground.GetCanvas();
        // foregroundCanvas.ResetState();
        // foregroundCanvas.Clear(Color.Transparent);
        // World.RenderForegroundLayer(foregroundCanvas);

        // World.RenderOverlayLayer(foregroundCanvas);

        DebugDraw.Draw(canvas, World.Camera);

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
        
        // canvas.PopState();
    }
}

class CompositingShader : CanvasShader
{
    public ITexture foregroundTexture;
    public ITexture visibiltyTexture;

    public CompositingShader()
    {
        noise = new int[1024];
        for (int i = 0; i < noise.Length; i++)
        {
            noise[i] = Random.Shared.Next();
        }
    }

    private int[] noise;
    public float Time = 0;
    public float Brightness = .45f;
    public float Noisiness = .15f;
    public bool renderNoise = false;

    public override ColorF GetPixelColor(Vector2 position)
    {
        float a = visibiltyTexture.Sample(position).A;
        ColorF fg = foregroundTexture.Sample(position) * a;
        ColorF noise = renderNoise ? new ColorF(0, 0, 0, CalcNoise(position) * (1 - a)) : ColorF.Transparent;
        return fg + noise;
    }

    private float CalcNoise(Vector2 position)
    {
        int seed = (int)(position.X * 17) ^ (int)(position.Y * 14);
        int offset = noise[(int)(seed + Time * 25) % noise.Length];
        float b = noise[offset % noise.Length] / (float)int.MaxValue;
        return 1 - (Brightness + (b * Noisiness));
    }

    public void Composite(ICanvas targetCanvas, ITexture texture, ITexture visibilityTexture, bool renderNoise)
    {
        this.foregroundTexture = texture;

        targetCanvas.PushState();
        targetCanvas.ResetState();
        this.foregroundTexture = texture;
        this.visibiltyTexture = visibilityTexture;
        this.Time = SimulationFramework.Time.TotalTime;
        this.renderNoise = renderNoise;
        targetCanvas.Fill(this);
        targetCanvas.DrawRect(0, 0, targetCanvas.Width, targetCanvas.Height);
        targetCanvas.PopState();
    }
}