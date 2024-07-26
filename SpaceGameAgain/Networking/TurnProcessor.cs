using SpaceGame.Commands;
using SpaceGame.Networking.Packets;
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

    public long CurrentTurn => turn;
    
    private long turn = -TurnDelay;
    private Dictionary<long, TurnInfo> turns = [];

    public bool TryPerformTurn()
    {
        if (turn < 0)
        {
            World.CommandProcessor.Flush();
            turn++;
            return true;
        }

        if (!turns.TryGetValue(turn, out TurnInfo? info))
        {
            // we don't have all turn the data yet
            return false;
        }

        if (info.playerCommands.Count != Program.Lobby!.players.Count)
        {
            // we don't have all turn the data yet
            return false;
        }

        // do actual turn
        foreach (var (player, commands) in info.playerCommands)
        {
            foreach (var command in commands)
            {
                command.Apply();
            }
        }

        World.CommandProcessor.Flush();
        turn++;
        return true;
    }

    public void SubmitCommands(long turn, int playerId, Command[] commands)
    {
        var packet = new CommandListPacket()
        {
            turn = turn,
            playerId = playerId,
            Commands = commands,
        };
        SubmitCommands(packet);
        Program.Lobby.network.SendPacket(packet);
    }

    public void SubmitCommands(CommandListPacket commandList)
    {
        if (!turns.TryGetValue(commandList.turn, out TurnInfo? info))
        {
            info = new();
            turns.Add(commandList.turn, info);
        }
        Console.WriteLine($"got commands for turn {commandList.turn}, player {commandList.playerId}");
        info.playerCommands.Add(commandList.playerId, commandList.Commands);
    }

}

class TurnInfo
{
    public Dictionary<int, Command[]> playerCommands = [];
}