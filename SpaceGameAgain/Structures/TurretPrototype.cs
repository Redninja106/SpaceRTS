﻿using SpaceGame.Combat;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class TurretPrototype : StructurePrototype
{
    public WeaponSystemPrototype WeaponSystemPrototype { get; set; }

    public override Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    {
        var turret = new Turret(this, id, grid, location, rotation, team);
        var weaponSystem = WeaponSystemPrototype.CreateWeapon(World.NewID(), ((Unit)turret).AsReference());
        World.Add(weaponSystem);

        return turret;
    }

    public override WorldActor Deserialize(BinaryReader reader)
    {
        base.DeserializeArgs(reader, out var id, out var team, out var grid, out var location, out var rotation);
        ActorReference<WeaponSystem> weaponSystem = reader.ReadActorReference<WeaponSystem>();

        return new Turret(this, id, grid, location, rotation, team)
        {
            weaponSystem = weaponSystem,
        };
    }
}
