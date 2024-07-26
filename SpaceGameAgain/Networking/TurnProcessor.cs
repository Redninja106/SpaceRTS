using SpaceGame.Commands;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class TurnProcessor
{
    public const long TurnDelay = 2;

    long turn = 0;

    public void Tick()
    {
        turn++;
    }
}

class TurnInfo
{
    Dictionary<Team, Queue<Command[]>> commands = [];
}