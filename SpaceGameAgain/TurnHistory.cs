using SpaceGame.Commands;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class TurnHistory
{
    private SortedDictionary<ulong, Dictionary<Team, Command[]>> turns = [];

    public Dictionary<Team, Command[]> GetTurn(ulong turn) => turns[turn];

    public void CommitTurn(ulong turn, Dictionary<Team, Command[]> commands)
    {
        this.turns.Add(turn, commands);
    }

    public void AddCommands(ulong turn, Team team, Command[] command)
    {
        if (!turns.TryGetValue(turn, out var turnDict))
        {
            turns[turn] = turnDict = [];
        }

        if (!turnDict.TryGetValue(team, out var prevCmds))
        {
            turnDict[team] = prevCmds = [];
        }

        turnDict[team] = [.. prevCmds, .. command];
    }

    public void Trim(ulong cutoff)
    {
        List<ulong> oldTurns = [];
        foreach (var (turn, _) in turns)
        {
            if (turn < cutoff)
            {
                oldTurns.Add(turn);
            }
        }

        foreach (var oldTurn in oldTurns)
        {
            turns.Remove(oldTurn);
        }
    }

    internal void Serialize(BinaryWriter writer)
    {
        writer.Write(turns.Count);
        foreach (var (turn, teamCommands) in turns)
        {
            writer.Write(turn);
            writer.Write(teamCommands.Count);
            foreach (var (team, commands) in teamCommands)
            {
                writer.Write(team.AsReference());

                writer.Write(commands.Length);
                foreach (var command in commands)
                {
                    Program.NetworkSerializer.Serialize(command, writer);
                }
            }
        }
    }

    internal static TurnHistory Deserialize(BinaryReader reader)
    {
        Dictionary<ulong, Dictionary<Team, Command[]>> turns = [];

        int turnCount = reader.ReadInt32();
        for (int i = 0; i < turnCount; i++)
        {
            ulong turn = reader.ReadUInt64();
            Dictionary<Team, Command[]> teams = [];
            int teamCount = reader.ReadInt32();
            for (int j = 0; j < teamCount; j++)
            {
                ActorReference<Team> team = reader.ReadActorReference<Team>();

                List<Command> commands = [];
                int commandCount = reader.ReadInt32();
                for (int k = 0; k < commandCount; k++)
                {
                    Command command = (Command)Program.NetworkSerializer.Deserialize(reader);
                    commands.Add(command);
                }

                teams.Add(team.Actor!, commands.ToArray());
            }
            turns.Add(turn, teams);
        }

        return new TurnHistory()
        {
            turns = new(turns)
        };
    }
}
