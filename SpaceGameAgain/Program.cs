
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
using SpaceGame.Scenes;
using SpaceGame.Networking;

// ImGuiConsole.SetIntercepts();
running = true;
DesktopPlatform.Register();
Start<Program>();
running = false;

partial class Program : Simulation
{
    public static IFont font;
    public static Scene Scene { get; private set; } = new MainMenuScene();
    private static Func<Scene>? nextScene;
    public static HostLobby? Host;
    public static ClientLobby? Lobby;
    public static bool running;
    public static float TickDelta = 1f/50f;

    public static void SwitchScenes(Func<Scene> scene)
    {
        nextScene = scene;
    }

    public override void OnInitialize()
    {
        Time.MaxDeltaTime = 1 / 30f;
        font ??= Graphics.LoadFont("Assets/Fonts/VictorMono-Regular.ttf");
    }

    float t;
    float timeSinceLastTick;

    public override void OnRender(ICanvas canvas)
    {
        Host?.Update();
        Lobby?.Update();
        t += Time.DeltaTime;
        while (t > 0)
        {
            Scene.Tick();
            t -= 1 / 50f;
            timeSinceLastTick = 0;
        }
        Scene.Update(Math.Clamp(timeSinceLastTick * 50f, 0, 1));
        timeSinceLastTick += Time.DeltaTime;
        Scene.Render(canvas);
        if (nextScene != null)
        {
            Scene = nextScene();
            nextScene = null;
        }
        ImGuiConsole.Layout();
    }
}
