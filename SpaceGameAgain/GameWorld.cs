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
using SpaceGame.Serialization;
using System.Collections;

namespace SpaceGame;
internal class GameWorld
{
    public static GameWorld World { get; set; }

    public static int TickRate { get; set; }

    public ActorList<Ship> Ships { get; } = [];
    public ActorList<StructureInstance> StructureInstances { get; } = [];
    public ActorList<Planet> Planets { get; } = [];
    public ActorList<Station> Stations { get; } = [];
    public ActorList<Grid> Grids { get; } = [];
    public ActorList<Team> Teams { get; } = [];
    public ActorList<Missile> Missiles { get; } = [];
    public ActorList<ChaingunRound> ChaingunRounds { get; } = [];
    public ActorList<Asteroid> Asteroids { get; } = [];

    // GLOBALS
    public Camera Camera { get; set; }
    
    public Team PlayerTeam { get; set; }

    public Sidebar LeftSidebar;
    public Sidebar RightSidebar;

    public Vector2 MousePosition;
    public bool HasFocus;

    public int NextID { get; set; } = 1;

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

    public GameWorld(WorldProvider provider)
    {
    }

    public void Update(float tickProgress, Vector2 viewportMousePosition, bool hasFocus)
    {
        if (Keyboard.IsKeyReleased(Key.F2))
        {
            using var fs = new FileStream("save.gxy", FileMode.Create);
            Save(fs);
        }
        else if (Keyboard.IsKeyReleased(Key.F3))
        {
            NetworkMap.Clear();
            using var fs = new FileStream("save.gxy", FileMode.Open);
            Load(fs);
        }

        this.HasFocus = hasFocus;
        
        MousePosition = Camera.ScreenToWorld(viewportMousePosition);

        leftMouse.Update();
        rightMouse.Update();

        MouseDragHandler.Update();

        CurrentInteractionContext ??= SelectInteractionContext;
        CurrentInteractionContext.Update(leftMouse, rightMouse);

        var soi = World.GetSphereOfInfluence(Camera.Transform.Position);
        soi?.ApplyTo(ref Camera.Transform);

        Planets.Update(tickProgress);
        Stations.Update(tickProgress);
        Ships.Update(tickProgress);
        ChaingunRounds.Update(tickProgress);
        Missiles.Update(tickProgress);
        Asteroids.Update(tickProgress);

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

        Planets.Tick();
        Ships.Tick();
        ChaingunRounds.Tick();
        Missiles.Tick();

        ticksUntilNextTurn--;
    }

    public void Render(ICanvas canvas)
    {
        Planets.Render(canvas);
        Stations.Render(canvas);

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

        Ships.Render(canvas);
        ChaingunRounds.Render(canvas);
        Missiles.Render(canvas);
        Asteroids.Render(canvas);
        
        CurrentInteractionContext ??= SelectInteractionContext;
        CurrentInteractionContext.Render(canvas, leftMouse, rightMouse);
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

    public void Save(Stream stream)
    {
        // TODO: add versioning
        // SaveActorList(stream, Teams);
        // SaveActorList(stream, Planets);
        // SaveActorList(stream, Ships);
    }

    public void Load(Stream stream)
    {
        // LoadActorList(stream, Teams);
        // LoadActorList(stream, Planets);
        // LoadActorList(stream, Ships);
        // 
        // PlayerTeam = Teams[0];
    }

    private void SaveActorList<TActor>(Stream stream, List<TActor> list)
        where TActor : Actor
    {
        stream.WriteValue(list.Count);
        foreach (var actor in list)
        {
            actor.Save(stream);
        }
    }

    private void LoadActorList<TActor>(Stream stream, List<TActor> list)
        where TActor : Actor, new()
    {
        list.Clear();
        int count = stream.ReadValue<int>();
        for (int i = 0; i < count; i++)
        {
            var actor = new TActor();
            actor.Load(stream);
            list.Add(actor);
        }
    }

    public uint ComputeCRC()
    {
        uint crc = 0;

        crc = Planets.ComputeCRC(crc);
        crc = Ships.ComputeCRC(crc);
        crc = ChaingunRounds.ComputeCRC(crc);
        crc = Missiles.ComputeCRC(crc);

        return crc;
    }
}

class ActorList<TActor> : IEnumerable<TActor>
    where TActor : Actor
{
    private List<TActor> actors = [];

    public void Add(TActor actor)
    {
        Add(actor, World.NextID++);
    }

    public void Add(TActor actor, int id)
    {
        actors.Add(actor);
        World.NetworkMap.Register(actor, id);
    }

    public void AddRange(ReadOnlySpan<TActor> actor)
    {
        actors.AddRange(actor);
    }

    public void Tick()
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

    public void Update(float tickProgress)
    {
        for (int i = 0; i < actors.Count; i++)
        {
            actors[i].Update(tickProgress);
        }
    }

    public void Render(ICanvas canvas)
    {
        foreach (var actor in actors)
        {
            canvas.PushState();
            actor.TweenedTransform.ApplyTo(canvas);
            actor.Render(canvas);
            canvas.PopState();
        }
    }

    public uint ComputeCRC(uint crc)
    {
        foreach (var actor in actors)
        {
            crc = actor.ComputeCRC(crc);
        }
        return crc;
    }

    public IEnumerator<TActor> GetEnumerator()
    {
        return ((IEnumerable<TActor>)actors).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)actors).GetEnumerator();
    }
}