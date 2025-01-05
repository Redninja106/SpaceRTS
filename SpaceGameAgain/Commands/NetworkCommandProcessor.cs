using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
internal class NetworkCommandProcessor : ICommandProcessor
{
    public Dictionary<ulong, Command[]> commands = [];

    public Command[] GetCommands(ulong turn)
    {
        return commands[turn];
    }

    public void AddCommands(ulong turn, Command[] commands)
    {
        this.commands.Add(turn, commands);
    }

    public bool HasCommands(ulong turn)
    {
        return commands.ContainsKey(turn);
    }

    public void RemoveCommands(ulong turn)
    {
        commands.Remove(turn);
    }
}
