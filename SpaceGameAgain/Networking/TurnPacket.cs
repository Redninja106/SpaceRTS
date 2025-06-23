using SpaceGame.Commands;
using SpaceGame.Debugging;
using SpaceGame.Extensions;
using SpaceGame.Networking.Packets;
using SpaceGame.Orders;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;

[Serializable]
internal class TurnPacket : Packet
{
    [Serialize]
    public required ulong turn;
    [Serialize]
    public required Team team;
    [Serialize]
    public required List<Command> commands;
    [Serialize]
    public required List<Command> prevTurnCommands;

    public void Process()
    {
        if (team.GetCommandProcessor() is not NetworkCommandProcessor commandProcessor)
        {
            // Debug.Assert(false);
            DebugLog.Warning($"Received commands for player controlled team {team.ID}. Dropping commands.");
            return;
        }

        if (this.turn < Program.World.TurnProcessor.turn)
        {
            return;
        }

        if (!commandProcessor.HasCommands(turn))
        {
            commandProcessor.AddCommands(turn, commands.ToArray());
        }
        else
        {
            DebugLog.Warning($"received turn {turn} for {team} twice!");
        }

        if (Program.World.TurnProcessor.turn < turn && !commandProcessor.HasCommands(turn - 1))
        {
            commandProcessor.AddCommands(turn - 1, prevTurnCommands.ToArray());
        }
        
        // Console.WriteLine("got commands for turn " + turn);
    }
}
