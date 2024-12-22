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
    WeaponSystem system;

    public Turret(TurretPrototype prototype, ulong id, Grid grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid, location, rotation, team)
    {
        if (prototype.TurretKind == "missile")
        {
            system = new MissileSystem(null, World.NewID(), Transform.Default, this);
        }
        else
        {
            system = new ChaingunSystem(null, World.NewID(), Transform.Default, this);
        }
    }

    public override void Update()
    {
        system.Update();
        base.Update();
    }
}
