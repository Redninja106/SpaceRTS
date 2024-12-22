using SpaceGame.GUI;
using SpaceGame.Planets;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal abstract class Unit(UnitPrototype prototype, ulong id, Transform transform, ActorReference<Team> team) : Actor(prototype, id, transform), IDestructable
{
    public ActorReference<Team> Team { get; set; } = team;
    public int Health { get; set; } = prototype.MaxHealth;

    bool IDestructable.IsDestroyed => Health <= 0;

    public virtual void OnDestroyed()
    {
    }

    public virtual Element[]? GetSelectionGUI()
    {
        return null;
    }

    public abstract bool TestPoint(Vector2 point, Transform transform);
}
