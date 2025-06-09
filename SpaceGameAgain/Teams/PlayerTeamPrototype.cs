using SpaceGame.Commands;
using SpaceGame.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Teams;
internal class PlayerTeamPrototype : TeamPrototype
{
    public override ICommandProcessor CreateCommandProcessor(Team team)
    {
        if (team == World.PlayerTeam.Actor)
        {
            return new PlayerCommandProcessor();
        }
        else
        {
            return new NetworkCommandProcessor();
        }
    }
}
