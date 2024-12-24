using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal abstract class WeaponSystem : Actor
{
    public ActorReference<Unit> unit;

    protected WeaponSystem(Prototype prototype, ulong id, ActorReference<Unit> unit) : base(prototype, id, Transform.Default)
    {
        this.unit = unit;
    }

    public abstract void Update();
}
