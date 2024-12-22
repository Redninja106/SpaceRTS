﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpaceGame.Interaction;
using SpaceGame.Ships;
using SpaceGame.Planets;
using SpaceGame.Structures;
using SpaceGame.Teams;
using SpaceGame.Combat;
using SpaceGame.GUI;
using System.Threading.Channels;
using Silk.NET.OpenGL;
using SimulationFramework.Desktop;

namespace SpaceGame;
internal class GameWorld
{
    public static GameWorld World { get; set; }

    public ulong NextID { get; set; } = 1;

    public Dictionary<ulong, Actor> Actors = [];


    public List<Ship> Ships { get; } = [];
    public List<Planet> Planets { get; } = [];
    public List<Team> Teams { get; } = [];
    public List<Missile> Missiles { get; } = [];
    public List<Bullet> Bullets { get; } = [];
    public List<Structure> Structures { get; } = [];

    //public List<Station> Stations { get; } = [];
    // public List<Asteroid> Asteroids { get; } = [];

    // GLOBALS
    public Camera Camera { get; set; } = new FreeCamera();

    public ActorReference<Team> PlayerTeam;
    public Team NeutralTeam { get; set; }

    public Sidebar LeftSidebar;
    public Sidebar RightSidebar;

    public Vector2 MousePosition;
    public bool HasFocus;

    // HANDLERS

    public SelectionHandler SelectionHandler { get; } = new();
    public MouseDragHandler MouseDragHandler { get; } = new();
    public SelectInteractionHandler SelectInteractionContext { get; } = new();
    public ConstructionInteractionContext ConstructionInteractionContext { get; } = new();

    public IInteractionContext? CurrentInteractionContext { get; set; }

    public MouseState leftMouse = new(MouseButton.Left);
    public MouseState rightMouse = new(MouseButton.Right);

    // public StructureList Structures { get; } = new();

    public IMask WorldShadowMask { get; private set; }
    public ElementWindow InfoWindow { get; set; } = new()
    {
        Anchor = Alignment.BottomRight,
        Width = 240,
        Height = 120,
    }; 
    public ElementWindow MapWindow { get; set; } = new()
    {
        Anchor = Alignment.BottomLeft,
        Width = 240,
        Height = 120,
    };

    public GameWorld()
    {
    }

    public void Update(Vector2 viewportMousePosition, bool hasFocus)
    {
        this.HasFocus = hasFocus;
        foreach (var planet in Planets)
        {
            planet.UpdateOrbit();
        }

        MousePosition = Camera.ScreenToWorld(viewportMousePosition);

        leftMouse.Update();
        rightMouse.Update();

        MouseDragHandler.Update();

        CurrentInteractionContext ??= SelectInteractionContext;
        CurrentInteractionContext.Update(leftMouse, rightMouse);

        var soi = World.GetSphereOfInfluence(Camera.Transform.Position);
        soi?.ApplyTo(ref Camera.Transform);

        UpdateActorList(Planets);
        UpdateActorList(Structures);
        //UpdateActorList(Stations);
        UpdateActorList(Ships);
        UpdateActorList(Bullets);
        UpdateActorList(Missiles);
        //UpdateActorList(Asteroids);

        foreach (var planet in Planets)
        {
            planet.SphereOfInfluence.Update();
        }
    }

    public void Render(ICanvas canvas)
    {
        if (WorldShadowMask is null || WorldShadowMask.Width != canvas.Width)
        {
            WorldShadowMask?.Dispose();
            WorldShadowMask = Graphics.CreateMask(canvas.Width, canvas.Height);
        }

        WorldShadowMask.Clear(true);

        RenderActorList(Planets, canvas);
        //RenderActorList(Stations, canvas);

        foreach (var planet in Planets)
        {
            canvas.PushState();
            planet.Transform.ApplyTo(canvas);
            planet.Grid.Render(canvas);
            canvas.PopState();
        }

        foreach (var structure in Structures)
        {
            canvas.PushState();
            structure.Transform.ApplyTo(canvas);
            canvas.Mask(World.WorldShadowMask);
            canvas.WriteMask(World.WorldShadowMask, false);
            structure.RenderShadow(
                canvas, 
                Vector2.TransformNormal(structure.Transform.Position.Normalized() * .4f, 
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
        RenderActorList(Bullets, canvas);
        RenderActorList(Missiles, canvas);
        //RenderActorList(Asteroids, canvas);
        CurrentInteractionContext ??= SelectInteractionContext;
        CurrentInteractionContext.Render(canvas, leftMouse, rightMouse);
    }

    private void UpdateActorList<TActor>(List<TActor> actors)
        where TActor : Actor
    {
        for (int i = 0; i < actors.Count; i++)
        {
            actors[i].Update();

            if (actors[i] is IDestructable destructable && destructable.IsDestroyed)
            {
                destructable.OnDestroyed();
                actors.RemoveAt(i);
                i--;
            }
        }
    }

    private void RenderActorList<TActor>(List<TActor> actors, ICanvas canvas)
        where TActor : Actor
    {
        foreach (var actor in actors)
        {
            canvas.PushState();
            actor.Transform.ApplyTo(canvas);
            actor.Render(canvas);
            canvas.PopState();
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

    public void Add(Actor actor)
    {
        Actors.Add(actor.ID, actor);

        if (actor is Ship s) Ships.Add(s);
        if (actor is Structure t) Structures.Add(t);
        if (actor is Planet p) Planets.Add(p);
        if (actor is Team e) Teams.Add(e);
        if (actor is Bullet b) Bullets.Add(b);
        if (actor is Missile m) Missiles.Add(m);

    }

    public ulong NewID()
    {
        return NextID++;
    }
}

class CollisionManager
{
    private Dictionary<(int, int), HashSet<Unit>> bins = [];
}