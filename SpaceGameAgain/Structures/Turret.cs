using SpaceGame.Combat;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class Turret : Structure
{
    public ActorReference<WeaponSystem> weaponSystem;

    public Turret(TurretPrototype prototype, ulong id, ActorReference<Grid> grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid, location, rotation, team)
    {
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(weaponSystem);
    }
}
