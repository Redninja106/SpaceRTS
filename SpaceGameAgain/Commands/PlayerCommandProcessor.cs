using SpaceGame.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
internal class PlayerCommandProcessor : ICommandProcessor
{
    public Dictionary<ulong, Command[]> commands = [];

    public Command[] GetCommands(ulong turn)
    {
        return commands.TryGetValue(turn, out Command[]? cmds) ? cmds : [];
    }

    public void AddCommand(Command command)
    {
        ulong turn = World.TurnProcessor.turn + TurnProcessor.TurnDelay;

        if (!commands.TryGetValue(turn, out Command[]? cmds))
        {
            cmds = [];
        }

        commands[turn] = [.. cmds, command];
    }

    public bool HasCommands(ulong turn)
    {
        return turn < (World.TurnProcessor.turn + TurnProcessor.TurnDelay);
    }

    public void RemoveCommands(ulong turn)
    {
        commands.Remove(turn);
    }

    public void BroadcastCommands(ulong turn)
    {
        if (Program.Lobby is null)
        {
            return;
        }

        var prototype = Prototypes.Get<TurnPacketPrototype>("turn_packet");
        var cmds = GetCommands(turn);
        var packet = new TurnPacket(prototype, turn, World.PlayerTeam, cmds.ToList(), GetCommands(turn-1).ToList());

        if (Program.Lobby is NetworkLobby networkLobby)
        {
            networkLobby.network.SendPacket(packet);
        }
        else if (Program.Lobby is LocalLobby localLobby)
        {
            localLobby.network.SendAll(packet);
        }
        else
        {
            throw new UnreachableException();
        }

        Console.WriteLine("send commands for turn " + turn);
    }
    
}
