using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal abstract class WeaponSystem : Actor
{
    protected WeaponSystem(Prototype prototype, ulong id, Transform transform) : base(prototype, id, transform)
    {
    }

    public abstract void Update();
}
