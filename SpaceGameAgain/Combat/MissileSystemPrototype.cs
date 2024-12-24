using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class MissileSystemPrototype : WeaponSystemPrototype
{
    public override Actor? Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        ActorReference<Unit> unit = reader.ReadActorReference<Unit>();
        ActorReference<Unit> target = reader.ReadActorReference<Unit>();
        int missilesRemaining = reader.ReadInt32();
        float timeSinceMissile = reader.ReadSingle();

        return new MissileSystem(this, id, unit)
        {
            MissilesRemaining = missilesRemaining,
            timeSinceMissile = timeSinceMissile,
            target = target
        };
    }

    public override WeaponSystem CreateWeapon(ulong id, ActorReference<Unit> unit)
    {
        return new MissileSystem(this, id, unit);
    }
}
