using SpaceGame.Debugging;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;

[Serializable]
internal class SummonShipCommand : Command
{
    [Serialize]
    public required Team team;
    [Serialize]
    public required ShipPrototype shipPrototype;
    [Serialize]
    public required ModulePrototype[] modulePrototypes;
    [Serialize]
    public required Transform transform;

    public override void Apply()
    {
        var ship = new Ship(shipPrototype, World.NewID()) { Team = team };
        ship.Teleport(transform);
        foreach (var modulePrototype in modulePrototypes)
        {
            var module = modulePrototype.CreateActor(World.NewID());
            module.Ship = ship;
            ship.modules.Add(module);
            World.Add(module);
        }
        World.Add(ship);

        DebugLog.Message("summoned ship " + ship.ID);
    }
}
