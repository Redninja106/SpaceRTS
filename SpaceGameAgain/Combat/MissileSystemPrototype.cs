using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class MissileSystemPrototype : WeaponSystemPrototype
{
    public int SalvoSize { get; set; } = 5;
    public int FireInterval { get; set; } = 25;
    public int SalvoInterval { get; set; } = 125;

    public override WorldActor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        ActorReference<Unit> unit = reader.ReadActorReference<Unit>();
        ActorReference<Unit> target = reader.ReadActorReference<Unit>();
        int missilesRemaining = reader.ReadInt32();
        int timeSinceMissile = reader.ReadInt32();

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
