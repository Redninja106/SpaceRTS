using SpaceGame.Commands;
using SpaceGame.Networking;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class TurnProcessor
{
    public const int TicksPerTurn = 3;
    public const long TurnDelay = 2;

    public int RemainingTicks = TicksPerTurn;
    public ulong turn;
    public ulong startingTurn = 0;

    public TurnHistory history = new();

    public TurnProcessor(ulong turn = 0)
    {
        this.turn = turn;
    }

    public bool ShouldTick()
    {
        RemainingTicks--;
        return RemainingTicks <= 0;
    }

    public bool TryPerformTurn()
    {
        if (RemainingTicks > 0)
        {
            throw new Exception("there are still ticks left in the turn");
        }

        if (turn < startingTurn + TurnDelay)
        {
            BroadcastCommands();

            turn++;
            RemainingTicks = TicksPerTurn;
            return true;
        }

        foreach (var team in World.Teams)
        {
            if (!team.CommandProcessor.HasCommands(turn))
            {
                return false;
            }
        }

        BroadcastCommands();

        Dictionary<Team, Command[]> capture = [];
        foreach (var team in World.Teams)
        {
            var commands = team.CommandProcessor.GetCommands(turn);
            capture[team] = commands;
            foreach (var command in commands)
            {
                command.Apply();
            }
        }

        history.CommitTurn(turn, capture);
        if (turn > 10)
        {
            history.Trim(turn - 10);
        }

        foreach (var team in World.Teams)
        {
            team.CommandProcessor.RemoveCommands(turn);
        }


        turn++;
        RemainingTicks = TicksPerTurn;

        return true;
    }

    public void BroadcastCommands()
    {
        PlayerCommandProcessor playerCommandProcessor = (PlayerCommandProcessor)World.PlayerTeam.Actor!.CommandProcessor;
        playerCommandProcessor.BroadcastCommands(turn + TurnDelay);

        // history.AddCommands(turn, team)
    }
}

class TurnInfo
{
    public SortedDictionary<ulong, List<Command>> playerCommands = [];
}