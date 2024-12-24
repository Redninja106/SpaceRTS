using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class ChaingunSystemPrototype : WeaponSystemPrototype
{
    public override Actor? Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        ActorReference<Unit> unit = reader.ReadActorReference<Unit>();
        int ammo = reader.ReadInt32();
        float angle = reader.ReadSingle();
        float timeSinceShot = reader.ReadSingle();

        return new ChaingunSystem(this, id, unit)
        {
            ammo = ammo,
            angle = angle,
            timeSinceShot = timeSinceShot,
        };
    }

    public override WeaponSystem CreateWeapon(ulong id, ActorReference<Unit> unit)
    {
        return new ChaingunSystem(this, id, unit);
    }
}
