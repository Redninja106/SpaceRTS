using SpaceGame.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Teams;
internal class NullTeamPrototype : TeamPrototype
{
    public override Type ActorType => typeof(Team);

    public override ICommandProcessor CreateCommandProcessor(GameWorld world, Team team)
    {
        return new NullCommandProcessor();
    }
}
