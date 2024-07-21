using SpaceGame.Planets;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal abstract class UnitBase : Actor, IDestructable
{
    public Team Team { get; set; }
    public bool IsDestroyed { get; set; }

    public abstract void Damage();
    public abstract bool TestPoint(Vector2 point, Transform transform);
}
