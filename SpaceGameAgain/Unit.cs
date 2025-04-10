using SpaceGame.Commands;
using SpaceGame.GUI;
using SpaceGame.Planets;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal abstract class Unit(UnitPrototype prototype, ulong id, Transform transform, ActorReference<Team> team) : WorldActor(prototype, id, transform), IDestructable, IGUIProvider
{
    public override UnitPrototype Prototype => (UnitPrototype)base.Prototype;

    public ActorReference<Team> Team { get; set; } = team;
    public int Health { get; set; } = prototype.MaxHealth;

    public abstract ITexture Icon { get; }

    bool IDestructable.IsDestroyed => Health <= 0;

    public virtual void OnDestroyed()
    {
    }

    public virtual Element[]? GetSelectionGUI()
    {
        return null;
    }

    public virtual double GetCollisionRadius()
    {
        return Prototype.CollisionRadius;
    }

    public virtual CommandPrototype[] GetCommands()
    {
        return [];
    }

    public abstract bool TestPoint(DoubleVector point);
    public abstract void Layout(GUIWindow window);
}

interface IGUIProvider
{
    ITexture Icon { get; }
    void Layout(GUIWindow window);
}
