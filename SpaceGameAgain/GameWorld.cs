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
using SpaceGame.Debugging;
using SpaceGame.Rendering;
using SpaceGame.Serialization;
using SpaceGame.Stations;
using System;

namespace SpaceGame;

public class GameWorld
{
    // public static GameWorld World { get; set; }

    public Dictionary<ulong, Actor> Actors = [];

    internal ActorList<Ship> Ships { get; }
    internal ActorList<Planet> Planets { get; }
    internal ActorList<Team> Teams { get; }
    internal ActorList<Missile> Missiles { get; }
    internal ActorList<Bullet> Bullets { get; }
    internal ActorList<Structure> Structures { get; }
    internal ActorList<Grid> Grids { get; }
    internal ActorList<WeaponSystem> WeaponSystems { get; }
    internal ActorList<Station> Stations { get; }
    
    //public List<Station> Stations { get; } = [];
    // public List<Asteroid> Asteroids { get; } = [];
    
    internal TextWidgetManager TextWidgets = new();

    internal UnitCollision Collision { get; }

    // GLOBALS
    public Camera Camera { get; set; } = new FreeCamera();


    // public Sidebar LeftSidebar;
    // public Sidebar RightSidebar;

    internal Team PlayerTeam;
    
    public ulong NextID { get; set; } = 1;

    public DoubleVector MousePosition;

    public ulong tick;
    public Random TickRandom;
    public ulong idleTicks;

    // HANDLERS
    internal SelectionHandler SelectionHandler { get; }
    internal MouseDragHandler MouseDragHandler { get; }
    internal SelectInteractionHandler SelectInteractionContext { get; }
    internal ConstructionInteractionContext ConstructionInteractionContext { get; }

    internal IInteractionContext? CurrentInteractionContext { get; set; }

    internal TurnProcessor TurnProcessor { get; }

    public MouseState leftMouse;
    public MouseState rightMouse;
    public MouseState middleMouse;

    private StarShader backgroundShader = new();

    // GUI
    // public ContextMenuWindow ContextMenu = new();

    // public ResourceBar ResourceBar = new();
    // public PopupWindow structureSelectWindow = new();

    // public TooltipManager Tooltip = new();


    // public WindowManager WindowManager = new WindowManager();

    internal GUIViewport GUIViewport = new GUIViewport();

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
        GUIViewport.Register(new GUIWindow(ResourceBar.Layout));
        GUIViewport.Register(new GUIWindow(UnitBar.Layout));

        Ships = new(this);
        Planets = new(this);
        Teams = new(this);
        Missiles = new(this);
        Bullets = new(this);
        Structures = new(this);
        Grids = new(this);
        WeaponSystems = new(this);
        Stations = new(this);

        Collision = new(this);

        SelectionHandler = new(this);
        MouseDragHandler = new(this);
        SelectInteractionContext = new(this);
        ConstructionInteractionContext = new(this);

        leftMouse = new(this, MouseButton.Left);
        rightMouse = new(this, MouseButton.Right);
        middleMouse = new(this, MouseButton.Middle);

        TurnProcessor = new(this);
    }

    public void Update(Vector2 viewportMousePosition, float tickProgress)
    {
        MousePosition = Camera.SmoothTransform.Position + DoubleVector.FromVector2(Camera.ScreenToLocal(viewportMousePosition));

        Planets.Update(tickProgress);

        var soi = GetSphereOfInfluence(Camera.Transform.Position);
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
        Stations.Update(tickProgress);
        Bullets.Update(tickProgress);
        Missiles.Update(tickProgress);
        
        foreach (var planet in Planets)
        {
            planet.SphereOfInfluence.Update();
        }
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

        SelectionHandler.Tick();

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
        Stations.Tick();
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
        // foreach (var (name, count) in PlayerTeam.Resources)
        // {
        //     MapWindow.Stack.Add(new Label($"{name}: {count}"));
        // }

        tick++;

        DebugMenu.PopMetric();
    }

    static readonly VisibilityShader visibilityShader = new VisibilityShader();

    public void RenderVisibility(ICanvas canvas)
    {
        canvas.Clear(Color.Transparent);
        RenderVisibility(Ships, canvas, Camera);
        RenderVisibility(Stations, canvas, Camera);
        RenderVisibility(Structures, canvas, Camera);

        void RenderVisibility<TUnit>(IList<TUnit> units, ICanvas canvas, Camera camera)
            where TUnit : Unit
        {
            foreach (var unit in units)
            {
                if (unit.CanReveal)
                {
                    canvas.PushState();
                    unit.InterpolatedTransform.ApplyTo(canvas, Camera);
                    visibilityShader.RevealRadius = (float)unit.GetRevealRadius();
                    canvas.Fill(visibilityShader);
                    Vector2 position = Vector2.Zero;
                    if (unit is Structure structure)
                    {
                        position = structure.Prototype.Center;
                    }
                    canvas.DrawCircle(position, visibilityShader.RevealRadius);
                    canvas.PopState();
                }
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
        WeaponSystems.Render(canvas, Camera);

        canvas.PopState();
    }

    public void RenderSkyLayer(ICanvas canvas)
    {
        canvas.PushState();

        Stations.Render(canvas, Camera);

        Ships.Sort((a, b) => a.Prototype.Scale.CompareTo(b.Prototype.Scale));
        Ships.Render(canvas, Camera);

        Missiles.Render(canvas, Camera);
        Bullets.Render(canvas, Camera);

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

    internal SphereOfInfluence? GetSphereOfInfluence(DoubleVector point)
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

    public IEnumerable<Actor> GetActorsByPrototype(Prototype prototype)
    {
        foreach (var a in Actors.Values)
        {
            if (a.Prototype == prototype)
            {
                yield return a;
            }
        }
    }

    public Actor CreateActor(Prototype prototype)
    {
        return prototype.CreateActor(this, this.NewID());
    }

    public void Add(Actor actor, bool skipInit = false)
    {
        Actors.Add(actor.ID, actor);

        Ships.AddIfApplicable(actor);
        Structures.AddIfApplicable(actor);
        Stations.AddIfApplicable(actor);
        Planets.AddIfApplicable(actor);
        Teams.AddIfApplicable(actor);
        Bullets.AddIfApplicable(actor);
        Missiles.AddIfApplicable(actor);
        Grids.AddIfApplicable(actor);
        WeaponSystems.AddIfApplicable(actor);

        if (!skipInit)
        {
            actor.InitializeActor();
        }
    }

    /// <summary>
    /// Allocates a new actor ID by incrementing the world's nextId counter.
    /// MUST ONLY BE CALLED IN SYNC (NEVER ON ONE CLIENT).
    /// </summary>
    public ulong NewID()
    {
        return NextID++;
    }

}
