using SpaceGame.Combat;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class TurretPrototype : StructurePrototype
{
    // public WeaponSystemPrototype WeaponSystemPrototype { get; set; }
    public WeaponSystemMount[] WeaponSystems { get; set; }

    public override Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    {
        var turret = new Turret(this, id, grid, location, rotation, team);
        turret.weaponSystems = new ActorReference<WeaponSystem>[WeaponSystems.Length];
        for (int i = 0; i < turret.weaponSystems.Length; i++)
        {
            turret.weaponSystems[i] = WeaponSystems[i].Prototype.CreateWeapon(World.NewID(), turret.AsReference<Unit>()).AsReference();
            turret.weaponSystems[i].Actor!.Offset = WeaponSystems[i].Offset;
            World.Add(turret.weaponSystems[i].Actor!);
        }

        return turret;
    }

    public override Actor Deserialize(BinaryReader reader)
    {
        base.DeserializeArgs(reader, out var id, out var team, out var grid, out var location, out var rotation);
        int weaponSystemCount = reader.ReadInt32();
        ActorReference<WeaponSystem>[] weaponSystems = new ActorReference<WeaponSystem>[weaponSystemCount];
        for (int i = 0; i < weaponSystemCount; i++)
        {
            weaponSystems[i] = reader.ReadActorReference<WeaponSystem>();
        }

        ActorReference<WeaponSystem> weaponSystem = reader.ReadActorReference<WeaponSystem>();

        return new Turret(this, id, grid, location, rotation, team)
        {
            weaponSystems = weaponSystems,
        };
    }
}