using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpaceGame.Interaction;
using SpaceGame.Ships;
using SpaceGame.Planets;
using SpaceGame.Stations;
using SpaceGame.Structures;
using SpaceGame.Teams;
using SpaceGame.Combat;
using SpaceGame.Asteroids;
using SpaceGame.GUI;
using System.Threading.Channels;
using Silk.NET.OpenGL;
using SimulationFramework.Desktop;
using SpaceGame.Networking;
using SpaceGame.Commands;

namespace SpaceGame;
internal class GameWorld
{
    public static GameWorld World { get; set; }

    public static int TickRate { get; set; }

    public List<Ship> Ships { get; } = [];
    public List<Planet> Planets { get; } = [];
    public List<Station> Stations { get; } = [];
    public List<Team> Teams { get; } = [];
    public List<Missile> Missiles { get; } = [];
    public List<ChaingunRound> ChaingunRounds { get; } = [];
    public List<Asteroid> Asteroids { get; } = [];

    // GLOBALS
    public Camera Camera { get; set; }
    
    public Team PlayerTeam { get; set; }

    public Sidebar LeftSidebar;
    public Sidebar RightSidebar;

    public Vector2 MousePosition;
    public bool HasFocus;

    // HANDLERS

    public SelectionHandler SelectionHandler { get; } = new();
    public MouseDragHandler MouseDragHandler { get; } = new();
    public SelectInteractionHandler SelectInteractionContext { get; } = new();
    public ConstructionInteractionContext ConstructionInteractionContext { get; } = new();
    public GameOverviewHandler OverviewHandler { get; } = new();

    public TurnProcessor TurnProcessor { get; } = new();
    public CommandProcessor CommandProcessor { get; } = new();

    public IInteractionContext? CurrentInteractionContext { get; set; }

    public MouseState leftMouse = new(MouseButton.Left);
    public MouseState rightMouse = new(MouseButton.Right);

    public StructureList Structures { get; } = new();
    public NetworkMap NetworkMap { get; } = new();

    public GameWorld()
    {

    }

    public void Update(float tickProgress, Vector2 viewportMousePosition, bool hasFocus)
    {
        this.HasFocus = hasFocus;
        
        MousePosition = Camera.ScreenToWorld(viewportMousePosition);

        leftMouse.Update();
        rightMouse.Update();

        MouseDragHandler.Update();

        CurrentInteractionContext ??= SelectInteractionContext;
        CurrentInteractionContext.Update(leftMouse, rightMouse);

        var soi = World.GetSphereOfInfluence(Camera.Transform.Position);
        soi?.ApplyTo(ref Camera.Transform);

        UpdateActorList(Planets, tickProgress);
        UpdateActorList(Stations, tickProgress);
        UpdateActorList(Ships, tickProgress);
        UpdateActorList(ChaingunRounds, tickProgress);
        UpdateActorList(Missiles, tickProgress);
        UpdateActorList(Asteroids, tickProgress);

        foreach (var planet in Planets)
        {
            planet.SphereOfInfluence.Update();
        }
    }

    public void Tick()
    {
        if (ticksUntilNextTurn <= 0)
        {
            if (TurnProcessor.TryPerformTurn())
            {
                ticksUntilNextTurn = 10;
            }
            else
            {
                return;
            }
        }

        foreach (var planet in Planets)
        {
            planet.TickOrbit();
        }

        TickActorList(Planets);
        TickActorList(Ships);

        ticksUntilNextTurn--;
    }

    public void Render(ICanvas canvas)
    {
        RenderActorList(Planets, canvas);
        RenderActorList(Stations, canvas);

        foreach (var planet in Planets)
        {
            canvas.PushState();
            planet.Transform.ApplyTo(canvas);
            planet.Grid.Render(canvas);
            canvas.PopState();
        }

        foreach (var ship in Ships)
        {
            canvas.PushState();
            ship.RenderShadow(canvas, 0);
            canvas.PopState();
        }

        RenderActorList(Ships, canvas);
        RenderActorList(ChaingunRounds, canvas);
        RenderActorList(Missiles, canvas);
        RenderActorList(Asteroids, canvas);
        
        CurrentInteractionContext ??= SelectInteractionContext;
        CurrentInteractionContext.Render(canvas, leftMouse, rightMouse);
    }

    private void UpdateActorList<TActor>(List<TActor> actors, float tickProgress)
        where TActor : Actor
    {
        for (int i = 0; i < actors.Count; i++)
        {
            actors[i].Update(tickProgress);
        }
    }

    private void RenderActorList<TActor>(List<TActor> actors, ICanvas canvas)
        where TActor : Actor
    {
        foreach (var actor in actors)
        {
            canvas.PushState();
            actor.TweenedTransform.ApplyTo(canvas);
            actor.Render(canvas);
            canvas.PopState();
        }
    }
    private void TickActorList<TActor>(List<TActor> actors)
        where TActor : Actor
    {
        for (int i = 0; i < actors.Count; i++)
        {
            actors[i].Tick();

            if (actors[i] is IDestructable destructable && destructable.IsDestroyed)
            {
                actors.RemoveAt(i);
                i--;
            }
        }
    }

    public SphereOfInfluence? GetSphereOfInfluence(Vector2 point)
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

    int ticksUntilNextTurn;
    internal float TickDelta = 1f/50f;

}