using SpaceGame.Commands;
using SpaceGame.GUI;
using SpaceGame.Interaction;
using SpaceGame.Planets;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal abstract class Unit(UnitPrototype prototype, ulong id, Transform transform, ActorReference<Team> team) : Actor(prototype, id, transform), IDestructable, IGUIProvider, ISelectable
{
    public override UnitPrototype Prototype => (UnitPrototype)base.Prototype;

    public ActorReference<Team> Team { get; set; } = team;
    public int Health { get; set; } = prototype.MaxHealth;
    public bool ClientVisible => World.tick - LastClientVisibleTick < 50;
    public ulong LastClientVisibleTick { get; set; }
    public virtual bool CanAttack => false;

    public abstract ITexture Icon { get; }

    bool IDestructable.IsDestroyed => Health <= 0;

    public virtual void OnDestroyed()
    {
    }

    //public virtual Element[]? GetSelectionGUI()
    //{
    //    return null;
    //}

    public override void Tick()
    {
        base.Tick();
    }

    public virtual double GetCollisionRadius()
    {
        return Prototype.CollisionRadius;
    }

    public virtual double GetRevealRadius()
    {
        return Prototype.RevealRadius;
    }

    public virtual CommandPrototype[] GetCommands()
    {
        return [];
    }

    public abstract bool TestPoint(DoubleVector point);
    public abstract void Layout(GUIWindow window);

    public virtual DoubleVector GetCenter()
    {
        return Transform.Position;
    }

    public virtual void DrawHighlightAbove(ICanvas canvas, Camera camera, bool selected)
    {
    }

    public virtual void DrawHighlightBelow(ICanvas canvas, Camera camera, bool selected)
    {
    }
}

interface IGUIProvider
{
    void Layout(GUIWindow window);
}
