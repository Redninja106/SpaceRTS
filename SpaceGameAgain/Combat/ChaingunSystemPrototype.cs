using SpaceGame.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class ChaingunSystemPrototype : WeaponSystemPrototype
{
    public float FireInterval { get; set; } = 2;
    public int AmmoCapacity { get; set; } = 150;
    public float TurnSpeed { get; set; } = 1;
    public float ReloadTime { get; set; } = 150;

    public override Actor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        ActorReference<Unit> unit = reader.ReadActorReference<Unit>();
        int ammo = reader.ReadInt32();
        float rotation = reader.ReadSingle();
        int timeSinceShot = reader.ReadInt32();

        var result = new ChaingunSystem(this, id, unit)
        {
            ammo = ammo,
            timeSinceShot = timeSinceShot,
        };
        result.Transform.Rotation = rotation;

        return result;
    }

    public override WeaponSystem CreateWeapon(ulong id, ActorReference<Unit> unit)
    {
        return new ChaingunSystem(this, id, unit);
    }
}
