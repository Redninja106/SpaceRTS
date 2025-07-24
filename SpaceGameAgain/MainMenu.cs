using Silk.NET.OpenGL;
using SpaceGame.GUI;
using SpaceGame.Planets;
using SpaceGame.Planets.Generation;
using SpaceGame.Rendering;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System.Reflection;

namespace SpaceGame;

class MainMenu : IScene
{
    public GUIViewport GUIViewport { get; }
    public Camera Camera { get; }

    private GUIWindow mainWindow;
    private GalaxyShader starShader;

    private BlackHoleShader blackHole;

    public MainMenu()
    {
        GUIViewport = new();
        
        Camera = new Camera();
        Camera.VerticalSize = 30000;
        Camera.InterpolationFactor = .001f;

        mainWindow = new(MainMenuLayout);
        GUIViewport.Register(mainWindow);

        starShader = new();
        starShader.Brightness = 1.0f;
       
        Reset();
    }

    public void Reset()
    {
        mainWindow.SetLayout(MainMenuLayout); 
        
        starShader.Galaxy = GalaxyInfo.Random(Random.Shared);
        starShader.Galaxy.turnSpeed = .025f * -float.Sign(starShader.Galaxy.ArmCurve);
        starShader.Galaxy.starDensity = 15;

        blackHole = new();
        blackHole.Radius = 100;

        Camera.VerticalSize = Camera.SmoothVerticalSize = 30000;
    }

    public void Update(Vector2 viewportMousePosition, float tickProgress)
    {
        if (mainWindow.Layout == CreateGameLayout)
        {
            Camera.VerticalSize = 500;
        }
        else
        {
            Camera.VerticalSize = 30000;
        }
    }
    
    public void Tick(Vector2 viewportMousePosition)
    {
    }

    public void RenderVisibility(ICanvas canvas)
    {
        canvas.Clear(Color.White);
    }

    public void RenderGroundLayer(ICanvas canvas)
    {
    }

    public void RenderSkyLayer(ICanvas canvas)
    {
    }

    public void RenderBackgroundLayer(ICanvas canvas)
    {
        starShader.Render(canvas, Camera);
        canvas.Fill(Color.Red);

        Transform.Default.ApplyTo(canvas, Camera);
        blackHole.Render(canvas);
    }

    public void RenderGroundOverlayLayer(ICanvas canvas)
    {
    }

    public void RenderSkyOverlayLayer(ICanvas canvas)
    {
    }

    public void RenderBackgroundOverlayLayer(ICanvas canvas)
    {
    }

    public static void MainMenuLayout(GUIWindow window)
    {
        window.Anchor = window.Alignment = Alignment.Center;

        using (window.Row())
        {
            window.Text("SPACEGAME", 32);
            window.Image(Icon.Get("ship_icon").Texture32x32, true);
        }

        window.Separator();
        if (window.TextButton("Singleplayer", fitArea: true, centerText: true))
        {
            window.SetLayout(CreateGameLayout);
        }
        window.TextButton("Multiplayer", fitArea: true, centerText: true);
        
        if (window.TextButton("Options", fitArea: true, centerText: true))
        {
            var optionsMenu = new OptionsMenu(MainMenuLayout);
            window.SetLayout(optionsMenu.Layout);
        }
        
        if (window.TextButton("Exit To Desktop", fitArea: true, centerText: true))
        {
            Application.Exit(true);
        }
    }

    public static void CreateGameLayout(GUIWindow window)
    {
        window.Anchor = window.Alignment = Alignment.Center;

        window.Text("Create Game", size: 24);
        window.Separator();

        if (window.TextButton("Create"))
        {
            Program.World = new();
            Program.SerializationContext = new(Program.World);

            var playerTeam = new Team(Prototypes.Get<PlayerTeamPrototype>("player_team"), Program.World, Program.World.NewID());
            playerTeam.Money += 1000;
            playerTeam.Unlock(Prototypes.Get<UnitPrototype>("headquarters"));
            Program.World.PlayerTeam = playerTeam;
            Program.World.Add(playerTeam);

            var starterShip = new Ship(Prototypes.Get<ShipPrototype>("small_ship"), Program.World, Program.World.NewID()) { Team = playerTeam };
            var module = new ConstructionModule(Prototypes.Get<ConstructionModulePrototype>("construction_module"), Program.World, Program.World.NewID()) { Ship = starterShip };
            starterShip.modules.Add(module);
            Program.World.Add(starterShip);
            Program.World.Add(module);

            var spacePirates = new Team(Prototypes.Get<TeamPrototype>("null_team"), Program.World, Program.World.NewID()) { Name = "Space Pirates" };
            Program.World.Add(spacePirates);

            GalaxyGenerator generator = new GalaxyGenerator();
            generator.Generate(Program.World, Random.Shared);

            Program.CurrentScene = Program.World;
        }
    }

}

class EscapeMenu
{
    public EscapeMenu()
    {

    }

    public void Layout(GUIWindow window)
    {
        window.Anchor = window.Alignment = Alignment.Center;
        window.MinSize(new(150, 0));
        if (!window.DummyLayout && Keyboard.IsKeyPressed(Key.Escape))
        {
            window.Visible = !window.Visible;
        }

        if (window.TextButton("resume", centerText: true, fitArea: true))
        {
            window.Visible = false;
        }

        if (window.TextButton("options", centerText: true, fitArea: true))
        {
            var optionsMenu = new OptionsMenu(Layout);
            window.SetLayout(optionsMenu.Layout);
        }

        if (window.TextButton("exit to main menu", centerText: true, fitArea: true))
        {
            Program.NavigateToMainMenu();
        }
    }
}

class OptionsMenu
{
    UserOptions options;
    UserOptions previousOptions;
    GUILayout returnLayout;

    public OptionsMenu(GUILayout returnLayout)
    {
        this.previousOptions = Program.UserOptions;   
        this.options = previousOptions.CreateCopy();
        Program.UserOptions = options;

        this.returnLayout = returnLayout;
    }

    public void Layout(GUIWindow window)
    {
        window.Anchor = window.Alignment = Alignment.Center;

        window.Text("Options", size: 24);
        window.Separator();

        using (window.Row())
        {
            window.Text("gui scale: " + this.options.GUIScale.ToString("F1"));
            if (window.TextButton("+"))
            {
                this.options.GUIScale += .1f;
                window.HasNewLayout = true;
            }
            if (window.TextButton("- "))
            {
                this.options.GUIScale -= .1f;
                this.options.GUIScale = float.Round(this.options.GUIScale, 1);
                window.HasNewLayout = true;
            }
        }

        using (window.Row())
        {
            window.Text("scroll speed: " + this.options.ScrollSpeed.ToString("F1"));
            if (window.TextButton("+"))
            {
                this.options.ScrollSpeed += .1f;
                window.HasNewLayout = true;
            }
            if (window.TextButton("-"))
            {
                this.options.ScrollSpeed -= .1f;
                this.options.ScrollSpeed = float.Round(this.options.ScrollSpeed, 1);
                window.HasNewLayout = true;
            }
        }

        using (window.Row())
        {
            window.Text($"display mode: {(Program.UserOptions.Fullscreen ? "fullscreen" : "windowed")}");
            if (window.LastItemHovered())
            {
                window.Viewport.SetTooltip(w => w.Text("click to toggle fullscreen"));
            }
            if (window.LastItemClicked(MouseButton.Left))
            {
                Program.UserOptions.Fullscreen = !Program.UserOptions.Fullscreen;
            }
        }

        using (window.Row())
        {
            window.Text($"vsync: {(Program.UserOptions.VSync ? "on" : "off")}");
            if (window.LastItemHovered())
            {
                window.Viewport.SetTooltip(w => w.Text("click to toggle vsync"));
            }
            if (window.LastItemClicked(MouseButton.Left))
            {
                this.options.VSync = !this.options.VSync;
                Graphics.SwapInterval = this.options.VSync ? 1 : 0;
            }
        }

        using (window.Row())
        {
            window.Text("chat fade delay: " + this.options.ChatFadeDelay.ToString());
            if (window.TextButton("+"))
            {
                this.options.ChatFadeDelay += 10;
                this.options.ChatFadeDelay = int.Clamp(this.options.ChatFadeDelay, 0, 500);
                window.HasNewLayout = true;
            }
            if (window.TextButton("-"))
            {
                this.options.ChatFadeDelay -= 10;
                this.options.ChatFadeDelay = int.Clamp(this.options.ChatFadeDelay, 0, 500);
                window.HasNewLayout = true;
            }
        }

        using (window.Row())
        {
            if (window.TextButton("cancel"))
            {
                window.SetLayout(returnLayout);
                Program.UserOptions = previousOptions;
            }

            if (window.TextButton("done"))
            {
                Program.UserOptions = this.options;
                Program.UserOptions.TrySave();
                window.SetLayout(returnLayout);
            }
        }
    }
}