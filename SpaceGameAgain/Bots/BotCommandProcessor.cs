using SpaceGame.AI;
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

    public BotCommandProcessor(Team team)
    {
        this.Team = team;

        this.UnitIndex = new();
        this.Enemies = new();
    }

    public void Think()
    {
        foreach (var strategy in Strategies)
        {
            strategy.Think();
        }
    }

    public void AddStrategy(Strategy strategy)
    {
    }

    public bool HasCommands(ulong turn)
    {
        throw new NotImplementedException();
    }

    public Command[] GetCommands(ulong turn)
    {
        throw new NotImplementedException();
    }

    public void RemoveCommands(ulong turn)
    {
        throw new NotImplementedException();
    }
}
