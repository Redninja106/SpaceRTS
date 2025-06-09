using SpaceGame.Interaction;
using SpaceGame.Ships;
using SpaceGame.Planets;
using SpaceGame.Structures;
using SpaceGame.Teams;
using SpaceGame.Combat;
using SpaceGame.GUI;
using ImGuiNET;
using System.Diagnostics;
using SpaceGame.Economy;
using SimulationFramework.Drawing.Shaders;

namespace SpaceGame;
internal class GameWorld
{
    public static GameWorld World { get; set; }

    public ulong NextID { get; set; } = 1;

    public Dictionary<ulong, WorldActor> Actors = [];

    public WorldActorList<Ship> Ships { get; } = [];
    public WorldActorList<Planet> Planets { get; } = [];
    public WorldActorList<Team> Teams { get; } = [];
    public WorldActorList<Missile> Missiles { get; } = [];
    public WorldActorList<Bullet> Bullets { get; } = [];
    public WorldActorList<Structure> Structures { get; } = [];
    public WorldActorList<Grid> Grids { get; } = [];
    public WorldActorList<WeaponSystem> WeaponSystems { get; } = [];
    
    //public List<Station> Stations { get; } = [];
    // public List<Asteroid> Asteroids { get; } = [];
    
    public TextWidgetManager TextWidgets = new();

    public UnitCollision Collision { get; } = new();

    // GLOBALS
    public Camera Camera { get; set; } = new FreeCamera();

    public ActorReference<Team> PlayerTeam;

    // public Sidebar LeftSidebar;
    // public Sidebar RightSidebar;

    public DoubleVector MousePosition;

    public ulong tick;
    public Random TickRandom;
    public ulong idleTicks;

    // HANDLERS
    public SelectionHandler SelectionHandler { get; } = new();
    public MouseDragHandler MouseDragHandler { get; } = new();
    public SelectInteractionHandler SelectInteractionContext { get; } = new();
    public ConstructionInteractionContext ConstructionInteractionContext { get; } = new();

    public IInteractionContext? CurrentInteractionContext { get; set; }

    public TurnProcessor TurnProcessor { get; } = new();

    public MouseState leftMouse = new(MouseButton.Left);
    public MouseState rightMouse = new(MouseButton.Right);
    public MouseState middleMouse = new(MouseButton.Middle);

    private StarShader backgroundShader = new();

    // GUI
    // public ContextMenuWindow ContextMenu = new();

    public UnitBar UnitBar = new();
    public ResourceBar ResourceBar = new();
    
    private TooltipWindow tooltipWindow = new();
    private Action<GUIWindow>? onLayoutTooltip = null;

    public PopupWindow structureSelectWindow = new();

    // public WindowManager WindowManager = new WindowManager();

    public GUIViewport GUIViewport = new GUIViewport();

    // public StructureList Structures { get; } = new();

    public IMask WorldShadowMask { get; private set; }
    public IMask FogOfWarMask { get; private set; }

    // public ElementWindow InfoWindow { get; set; } = new()
    // {
    //     Anchor = Alignment.BottomRight,
    //     Width = 240,
    //     Height = 240,
    // }; 
    // public ElementWindow MapWindow { get; set; } = new()
    // {
    //     Anchor = Alignment.BottomLeft,
    //     Width = 240,
    //     Height = 120,
    // };

    // public FogOfWarHandler FogOfWar { get; set; } = new();

    public GameWorld()
    {
        // WindowManager.RegisterWindow(ContextMenu);
        GUIViewport.Register(UnitBar);
        GUIViewport.Register(ResourceBar);
        GUIViewport.Register(structureSelectWindow);

        GUIViewport.Register(tooltipWindow);
        // GUIViewport.Register(ConstructionMenu);
        // GUIViewport.Register(InfoMenu);
        // WindowManager.RegisterWindow(UtilityBar);
    }

    public void Update(Vector2 viewportMousePosition, float tickProgress)
    {
        onLayoutTooltip = null;

        MousePosition = Camera.SmoothTransform.Position + DoubleVector.FromVector2(Camera.ScreenToLocal(viewportMousePosition));

        Planets.Update(tickProgress);

        var soi = World.GetSphereOfInfluence(Camera.Transform.Position);
        soi?.ApplyUpdateTo(ref Camera.Transform);
        soi?.ApplyUpdateTo(ref Camera.SmoothTransform);


        leftMouse.Update();
        rightMouse.Update();
        middleMouse.Update();

        MouseDragHandler.Update();
        
        CurrentInteractionContext ??= SelectInteractionContext;
        CurrentInteractionContext.Update(leftMouse, rightMouse);

        Grids.Update(tickProgress);
        Structures.Update(tickProgress);
        //UpdateActorList(Stations);
        WeaponSystems.Update(tickProgress);
        Ships.Update(tickProgress);
        Bullets.Update(tickProgress);
        Missiles.Update(tickProgress);
        
        foreach (var planet in Planets)
        {
            planet.SphereOfInfluence.Update();
        }

        foreach (var window in GUIViewport.windows)
        {
            window.Layout();
        }
        tooltipWindow.Visible = onLayoutTooltip != null;
        onLayoutTooltip?.Invoke(tooltipWindow);
    }

    public void Tick(Vector2 viewportMousePosition)
    {
        DebugMenu.PushMetric();

        if (Program.Lobby?.IsDownloadingWorld ?? false)
        {
            return;
        }

        if (TurnProcessor.RemainingTicks == 0)
        {
            if (!TurnProcessor.TryPerformTurn())
            {
                idleTicks++;
                return;
            }
            else
            {
                idleTicks = 0;
            }
        }
        else
        {
            TurnProcessor.RemainingTicks--;
        }

        TickRandom = new(unchecked((int)tick));

        World.SelectionHandler.Tick();

        Planets.Tick(); 
        
        foreach (var planet in Planets)
        {
            planet.TickOrbit();
        }

        leftMouse.Tick();
        rightMouse.Tick();
        middleMouse.Tick();

        // Collision.ClearBins();
        Collision.Update();
        

        Structures.Tick();
        Grids.Tick();
        //UpdateActorList(Stations);
        WeaponSystems.Tick();
        Ships.Tick();
        Bullets.Tick();
        Missiles.Tick();

        TextWidgets.Tick();

        //UpdateActorList(Asteroids);

        foreach (var planet in Planets)
        {
            planet.SphereOfInfluence.Tick();
        }

        DebugOverlays.Tick();

        // MapWindow.Stack.Clear();
        // foreach (var (name, count) in PlayerTeam.Actor!.Resources)
        // {
        //     MapWindow.Stack.Add(new Label($"{name}: {count}"));
        // }

        tick++;

        DebugMenu.PopMetric();
    }

    NoiseShader ns = new();

    public void Render(ICanvas canvas)
    {
        if (WorldShadowMask is null || WorldShadowMask.Width != canvas.Width)
        {
            WorldShadowMask?.Dispose();
            WorldShadowMask = Graphics.CreateMask(canvas.Width, canvas.Height);
        }

        WorldShadowMask.Clear(true);

        backgroundShader.Render(canvas, Camera);

        Planets.Render(canvas, Camera);
        //RenderActorList(Stations, canvas);

        Grids.Render(canvas, Camera);

        foreach (var structure in Structures)
        {
            canvas.PushState();
            structure.InterpolatedTransform.ApplyTo(canvas, Camera);
            canvas.Mask(World.WorldShadowMask);
            canvas.WriteMask(World.WorldShadowMask, false);
            structure.RenderShadow(
                canvas, 
                Vector2.TransformNormal(structure.InterpolatedTransform.Position.ToVector2().Normalized() * .4f, 
                Matrix3x2.CreateRotation(-structure.Rotation * (MathF.Tau / 6f)))
                );
            canvas.PopState();
        }

        Structures.Sort((a, b) => a.GetCenter().Y.CompareTo(b.GetCenter().Y));
        Structures.Render(canvas, Camera);

        foreach (var ship in Ships)
        {
            canvas.PushState();
            canvas.Mask(WorldShadowMask);
            canvas.WriteMask(WorldShadowMask, false);
            ship.RenderShadow(canvas, 0);
            canvas.PopState();
        }
        
        Ships.Render(canvas, Camera);
        Missiles.Render(canvas, Camera);
        Bullets.Render(canvas, Camera);
        WeaponSystems.Render(canvas, Camera);
        //RenderActorList(Asteroids, canvas);
        CurrentInteractionContext ??= SelectInteractionContext;
        CurrentInteractionContext.RenderBackgroundOverlay(canvas, leftMouse, rightMouse);


        // render objects - true->mask
        // render shadows - false->mask
        // render lights - 

        // background buffer
        // foreground buffer
        // fog buffer

        // render background
        // render foreground
        // render fog
        // blend based on fog buffer
    }

    public void RenderVisibility(ICanvas canvas)
    {
        canvas.Clear(Color.Transparent);
        VisibilityShader vs = new VisibilityShader();
        foreach (var ship in Ships)
        {
            if (ship.Team == this.PlayerTeam)
            {
                canvas.PushState();
                ship.InterpolatedTransform.ApplyTo(canvas, Camera);
                vs.RevealRadius = (float)ship.GetRevealRadius();
                canvas.Fill(vs);
                canvas.DrawCircle(Vector2.Zero, vs.RevealRadius);
                canvas.PopState();
            }
        }

        foreach (var structure in Structures)
        {
            if (structure.Team == this.PlayerTeam)
            {
                canvas.PushState();
                structure.InterpolatedTransform.ApplyTo(canvas, Camera);
                vs.RevealRadius = (float)structure.GetRevealRadius();
                canvas.Fill(vs);
                canvas.DrawCircle(structure.Prototype.Center, vs.RevealRadius);
                canvas.PopState();
            }
        }
    }

    class VisibilityShader : CanvasShader
    {
        public float RevealRadius = 5;

        public override ColorF GetPixelColor(Vector2 position)
        {
            //float d = RevealRadius - position.Length();
            return new ColorF(0, 0, 0, 1);
        }
    }

    public void RenderGroundLayer(ICanvas canvas)
    {
        canvas.PushState();
        
        Structures.Sort((a, b) => a.GetCenter().Y.CompareTo(b.GetCenter().Y));
        Structures.Render(canvas, Camera);

        canvas.PopState();
    }

    public void RenderSkyLayer(ICanvas canvas)
    {
        canvas.PushState();
        
        Ships.Sort((a, b) => a.Prototype.Scale.CompareTo(b.Prototype.Scale));
        Ships.Render(canvas, Camera);

        Missiles.Render(canvas, Camera);
        Bullets.Render(canvas, Camera);
        WeaponSystems.Render(canvas, Camera);

        TextWidgets.RenderEventWidgets(canvas, Camera);

        canvas.PopState();
    }

    public void RenderBackgroundLayer(ICanvas canvas)
    {
        canvas.PushState();
        
        backgroundShader.Render(canvas, Camera);
        Planets.Render(canvas, Camera);
        Grids.Render(canvas, Camera);

        SelectionHandler.RenderBackgroundOverlay(canvas, Camera);

        canvas.PopState();
    }

    public void RenderGroundOverlayLayer(ICanvas canvas)
    {
        canvas.PushState();

        SelectionHandler.RenderGroundOverlay(canvas, Camera);

        CurrentInteractionContext ??= SelectInteractionContext;
        CurrentInteractionContext.RenderGroundOverlay(canvas, leftMouse, rightMouse);

        canvas.PopState();
    }

    public void RenderSkyOverlayLayer(ICanvas canvas)
    {
        canvas.PushState();
        

        SelectionHandler.RenderSkyOverlay(canvas, Camera);

        CurrentInteractionContext ??= SelectInteractionContext;
        CurrentInteractionContext.RenderSkyOverlay(canvas, leftMouse, rightMouse);

        TextWidgets.RenderNotificationWidgets(canvas, Camera);

        canvas.PopState();
    }

    public void RenderBackgroundOverlayLayer(ICanvas canvas)
    {
        canvas.PushState();
        
        SelectionHandler.RenderBackgroundOverlay(canvas, Camera);
        
        CurrentInteractionContext ??= SelectInteractionContext;
        CurrentInteractionContext.RenderBackgroundOverlay(canvas, leftMouse, rightMouse);
       
        canvas.PopState();
    }

    public SphereOfInfluence? GetSphereOfInfluence(DoubleVector point)
    {
        SphereOfInfluence? smallest = null;
        foreach (var planet in Planets)
        {
            if (planet.SphereOfInfluence.ContainsPoint(point))
            {
                if (planet.SphereOfInfluence.Radius < (smallest?.Radius ?? float.PositiveInfinity))
                {
                    smallest = planet.SphereOfInfluence;
                }
            }
        }
        return smallest;
    }

    public IEnumerable<WorldActor> GetActorsByPrototype(WorldActorPrototype prototype)
    {
        foreach (var a in Actors.Values)
        {
            if (a.Prototype == prototype)
            {
                yield return a;
            }
        }
    }

    public void Add(WorldActor actor)
    {
        Actors.Add(actor.ID, actor);

        Ships.AddIfApplicable(actor);
        Structures.AddIfApplicable(actor);
        Planets.AddIfApplicable(actor);
        Teams.AddIfApplicable(actor);
        Bullets.AddIfApplicable(actor);
        Missiles.AddIfApplicable(actor);
        Grids.AddIfApplicable(actor);
        WeaponSystems.AddIfApplicable(actor);
    }

    /// <summary>
    /// Allocates a new actor ID by incrementing the world's nextId counter.
    /// MUST ONLY BE CALLED IN SYNC (NEVER ON ONE CLIENT).
    /// </summary>
    public ulong NewID()
    {
        return NextID++;
    }

    public void SetTooltip(Action<GUIWindow> onLayoutTooltip)
    {
        this.onLayoutTooltip = onLayoutTooltip;
    }

}
