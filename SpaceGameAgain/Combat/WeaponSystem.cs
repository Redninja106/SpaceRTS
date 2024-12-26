using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal abstract class WeaponSystem : Actor, IDestructable
{
    public ActorReference<Unit> unit;


    protected WeaponSystem(Prototype prototype, ulong id, ActorReference<Unit> unit) : base(prototype, id, Transform.Default)
    {
        this.unit = unit;
    }

    public bool IsDestroyed => ((IDestructable)unit.Actor!).IsDestroyed;

    public void OnDestroyed()
    {
    }
}
