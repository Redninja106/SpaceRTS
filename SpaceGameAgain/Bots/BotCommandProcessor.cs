using SpaceGame.Commands;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Bots;
internal class BotCommandProcessor : ICommandProcessor
{
    public Team Team { get; }

    private List<Strategy> Strategies = [];

    public FriendlyUnitIndex UnitIndex { get; }
    public Dictionary<Team, EnemyUnitIndex> Enemies { get; }

    private List<Command> commands = [];
    private bool hasCommands = false;

    public BotCommandProcessor(Team team)
    {
        this.Team = team;

        this.UnitIndex = new();
        this.Enemies = new();
    }

    public virtual void Think()
    {
        foreach (var strategy in Strategies)
        {
            strategy.Think();
            commands.AddRange(strategy.Commands);
            strategy.Commands.Clear();
        }

        hasCommands = true;
    }

    public void AddStrategy(Strategy strategy)
    {
        Strategies.Add(strategy);
    }

    public bool HasCommands(ulong turn)
    {
        return hasCommands;
    }

    public Command[] GetCommands(ulong turn)
    {
        return commands.ToArray();
    }

    public void RemoveCommands(ulong turn)
    {
        hasCommands = false;
    }
}
