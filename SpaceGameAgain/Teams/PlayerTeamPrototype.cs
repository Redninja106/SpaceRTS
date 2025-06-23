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
    public override Type ActorType => typeof(Team);

    public override ICommandProcessor CreateCommandProcessor(GameWorld world, Team team)
    {
        if (team == world.PlayerTeam)
        {
            return new PlayerCommandProcessor();
        }
        else
        {
            return new NetworkCommandProcessor();
        }
    }
}
