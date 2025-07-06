using SimulationFramework.Desktop;
using SpaceGame.Bots;
using SpaceGame.Commands;
using SpaceGame.Debugging;
using SpaceGame.GUI;
using SpaceGame.Interaction;
using SpaceGame.Networking;
using SpaceGame.Planets;
using SpaceGame.Planets.Generation;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
//using SpaceGame.Stations;
using SpaceGame.Structures;
using SpaceGame.Structures.Shipyards;
using SpaceGame.Teams;
using SpaceGame.Tiles;
using System.Diagnostics;
using System.Xml.Linq;

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
    public const int ViewportPixels = 480;

    public static IFont font;
    public static Vector2 ViewportMousePosition;
    public static float uiscale = 2f;
    public static float uiscaleResolutionFactor = 1 / 1920f;

    public const float TickRate = 50f;
    public const float Timestep = 1 / TickRate;
    public static float GameSpeed = 1f;
    public static bool forceTickThisFrame = false;

    public static float timeAccumulated = 0;

    public static Lobby? Lobby;

    public static float ViewportScale;

    private ITexture viewTexture;
    private ITexture groundTexture;
    private ITexture skyTexture;
    private ITexture visibilityTexture;
    private CompositingShader compositingShader = new CompositingShader();

    public static UserOptions UserOptions;

    public static GameWorld World;
    public static MainMenu MainMenu;
    public static IScene CurrentScene;
    public static SerializationContext SerializationContext;

    public override void OnInitialize()
    {
        Time.MaxDeltaTime = 1 / 30f;
        font ??= Graphics.LoadFont("Assets/Fonts/VictorMono-Regular.ttf");
        
        DebugOverlays.Register();
        Prototypes.Load();

        UserOptions = UserOptions.LoadOrCreate();
        
        MainMenu = new();
        CurrentScene = MainMenu;

        Graphics.SwapInterval = UserOptions.VSync ? 1 : 0;
    }


    public override void OnRender(ICanvas canvas)
    {
        Window.Title = "SpaceGame - " + Performance.Framerate.ToString("f0") + "FPS";
        if (Keyboard.IsKeyPressed(Key.F11))
        {
            UserOptions.Fullscreen = !UserOptions.Fullscreen;
        }

        if (!Window.IsMinimized && Window.IsFullscreen != UserOptions.Fullscreen) 
        {
            Window.ToggleFullscreen();
        }

        float aspectRatio = canvas.Width / (float)canvas.Height;
        int targetViewWidth = (int)(ViewportPixels * aspectRatio);

        if (viewTexture is null || viewTexture.Width != targetViewWidth)
        {
            viewTexture?.Dispose();
            viewTexture = Graphics.CreateTexture(targetViewWidth, ViewportPixels);

            visibilityTexture?.Dispose();
            visibilityTexture = Graphics.CreateTexture(targetViewWidth, ViewportPixels);

            groundTexture?.Dispose();
            groundTexture = Graphics.CreateTexture(targetViewWidth, ViewportPixels);

            skyTexture?.Dispose();
            skyTexture = Graphics.CreateTexture(targetViewWidth, ViewportPixels);
        }

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
        canvas.DrawTexture(this.viewTexture);
        canvas.PopState();

        CurrentScene.GUIViewport.Render(canvas);
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

        CurrentScene.Camera.Update(viewTexture.Width, viewTexture.Height, tickProgress);
        Rectangle vp = new(0, 0, viewTexture.Width, viewTexture.Height);

        ViewportMousePosition = Vector2.Transform(Mouse.Position, viewMatrix.InverseMatrix);
        
        if (forceTickThisFrame || GameSpeed >= 0 && timeAccumulated >= Timestep / GameSpeed)
        {
            forceTickThisFrame = false;
            DebugDraw.Clear();

            DebugMenu.ClearMetrics();
            CurrentScene.Tick(ViewportMousePosition);

            timeAccumulated = 0;
            tickProgress = 0;
        }

        CurrentScene.GUIViewport.UpdateWindowOcculusion(canvas.Width, canvas.Height);
        CurrentScene.Update(ViewportMousePosition, tickProgress);
        CurrentScene.GUIViewport.Update();
    }

    private void RenderViewTexture()
    {
        DebugMenu.PushMetric();

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
        CurrentScene.Camera.RenderSetup(canvas);

        // prep visibility texture for compositing the layers
        CurrentScene.RenderVisibility(visibilityCanvas);
        visibilityCanvas.Flush();

        // BACKGROUND LAYER
        CurrentScene.RenderBackgroundLayer(canvas);
        CurrentScene.RenderBackgroundOverlayLayer(canvas);

        // GROUND LAYER
        CurrentScene.RenderGroundLayer(groundCanvas);
        groundCanvas.Flush();
        compositingShader.Composite(canvas, groundTexture, visibilityTexture, false);
        CurrentScene.RenderGroundOverlayLayer(canvas);

        // SKY LAYER
        CurrentScene.RenderSkyLayer(skyCanvas);
        skyCanvas.Flush();
        compositingShader.Composite(canvas, skyTexture, visibilityTexture, true);
        CurrentScene.RenderSkyOverlayLayer(canvas);

        DebugMenu.PushMetric("DebugDraw.Draw");
        DebugDraw.Draw(canvas, CurrentScene.Camera);
        DebugMenu.PopMetric();

        DebugMenu.PopMetric();
    }

    public static void NavigateToMainMenu()
    {
        MainMenu.Reset();
        World = null;
        CurrentScene = MainMenu;
    }
}
