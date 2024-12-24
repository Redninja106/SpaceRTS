using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal abstract class WeaponSystemPrototype : Prototype
{
    public abstract WeaponSystem CreateWeapon(ulong id, ActorReference<Unit> unit);
}
