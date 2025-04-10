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

namespace SpaceGame;
internal class GameWorld
{
    public static GameWorld World { get; set; }

    public ulong NextID { get; set; } = 1;

    public Dictionary<ulong, WorldActor> Actors = [];

    public List<Ship> Ships { get; } = [];
    public List<Planet> Planets { get; } = [];
    public List<Team> Teams { get; } = [];
    public List<Missile> Missiles { get; } = [];
    public List<Bullet> Bullets { get; } = [];
    public List<Structure> Structures { get; } = [];
    public List<Grid> Grids { get; } = [];
    public List<WeaponSystem> WeaponSystems { get; } = [];

    //public List<Station> Stations { get; } = [];
    // public List<Asteroid> Asteroids { get; } = [];

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
    public TooltipWindow tooltipWindow = new();
    
    // public WindowManager WindowManager = new WindowManager();

    public GUIViewport GUIViewport = new GUIViewport();

    // public StructureList Structures { get; } = new();

    public IMask WorldShadowMask { get; private set; }

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
        GUIViewport.Register(tooltipWindow);
        // GUIViewport.Register(ConstructionMenu);
        // GUIViewport.Register(InfoMenu);
        // WindowManager.RegisterWindow(UtilityBar);
    }

    public void Update(Vector2 viewportMousePosition, float tickProgress)
    {
        MousePosition = DoubleVector.FromVector2(Camera.ScreenToWorld(viewportMousePosition));

        UpdateActorList(Planets, tickProgress);

        var soi = World.GetSphereOfInfluence(Camera.Transform.Position);
        soi?.ApplyUpdateTo(ref Camera.Transform);
        soi?.ApplyUpdateTo(ref Camera.SmoothTransform);

        leftMouse.Update();
        rightMouse.Update();
        middleMouse.Update();

        MouseDragHandler.Update();
        
        CurrentInteractionContext ??= SelectInteractionContext;
        CurrentInteractionContext.Update(leftMouse, rightMouse);

        UpdateActorList(Grids, tickProgress);
        UpdateActorList(Structures, tickProgress);
        //UpdateActorList(Stations);
        UpdateActorList(WeaponSystems, tickProgress);
        UpdateActorList(Ships, tickProgress);
        UpdateActorList(Bullets, tickProgress);
        UpdateActorList(Missiles, tickProgress);
        
        foreach (var planet in Planets)
        {
            planet.SphereOfInfluence.Update();
        }
    }

    public void Tick(Vector2 viewportMousePosition)
    {
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


        TickActorList(Planets); 
        
        foreach (var planet in Planets)
        {
            planet.TickOrbit();
        }

        leftMouse.Tick();
        rightMouse.Tick();

        Collision.ClearBins();
        Collision.Update();
        

        TickActorList(Structures);
        TickActorList(Grids);
        //UpdateActorList(Stations);
        TickActorList(WeaponSystems);
        TickActorList(Ships);
        TickActorList(Bullets);
        TickActorList(Missiles);
        //UpdateActorList(Asteroids);

        foreach (var planet in Planets)
        {
            planet.SphereOfInfluence.Tick();
        }


        // MapWindow.Stack.Clear();
        // foreach (var (name, count) in PlayerTeam.Actor!.Resources)
        // {
        //     MapWindow.Stack.Add(new Label($"{name}: {count}"));
        // }

        tick++;
    }

    public void Render(ICanvas canvas)
    {
        if (WorldShadowMask is null || WorldShadowMask.Width != canvas.Width)
        {
            WorldShadowMask?.Dispose();
            WorldShadowMask = Graphics.CreateMask(canvas.Width, canvas.Height);
        }

        WorldShadowMask.Clear(true);

        backgroundShader.Render(canvas, Camera);

        RenderActorList(Planets, canvas);
        //RenderActorList(Stations, canvas);

        RenderActorList(Grids, canvas);

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

        RenderActorList(Structures, canvas);

        foreach (var ship in Ships)
        {
            canvas.PushState();
            canvas.Mask(WorldShadowMask);
            canvas.WriteMask(WorldShadowMask, false);
            ship.RenderShadow(canvas, 0);
            canvas.PopState();
        }

        RenderActorList(Ships, canvas);
        RenderActorList(Missiles, canvas);
        RenderActorList(Bullets, canvas);
        RenderActorList(WeaponSystems, canvas);
        //RenderActorList(Asteroids, canvas);
        CurrentInteractionContext ??= SelectInteractionContext;
        CurrentInteractionContext.Render(canvas, leftMouse, rightMouse);

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

    private void UpdateActorList<TActor>(List<TActor> actors, float tickProgress)
        where TActor : WorldActor
    {
        for (int i = 0; i < actors.Count; i++)
        {
            actors[i].Update(tickProgress);
        }
    }

    private void TickActorList<TActor>(List<TActor> actors)
        where TActor : WorldActor
    {
        for (int i = 0; i < actors.Count; i++)
        {
            actors[i].Tick();

            if (actors[i] is IDestructable destructable && destructable.IsDestroyed)
            {
                destructable.OnDestroyed();
                actors.RemoveAt(i);
                i--;
            }
        }
    }

    private void RenderActorList<TActor>(List<TActor> actors, ICanvas canvas)
        where TActor : WorldActor
    {
        foreach (var actor in actors)
        {
            canvas.PushState();
            actor.InterpolatedTransform.ApplyTo(canvas, Camera);
            actor.Render(canvas);
            canvas.PopState();
        }
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

        if (actor is Ship s) Ships.Add(s);
        if (actor is Structure t) Structures.Add(t);
        if (actor is Planet p) Planets.Add(p);
        if (actor is Team e) Teams.Add(e);
        if (actor is Bullet b) Bullets.Add(b);
        if (actor is Missile m) Missiles.Add(m);
        if (actor is Grid g) Grids.Add(g);
        if (actor is WeaponSystem w) WeaponSystems.Add(w);

    }

    public ulong NewID()
    {
        return NextID++;
    }

    public void ClientInitialize(Team playerTeam)
    {
        this.PlayerTeam = playerTeam.AsReference();

        this.Camera = new FreeCamera();

    }
}

class CollisionManager
{
    private Dictionary<(int, int), HashSet<Unit>> bins = [];
}
