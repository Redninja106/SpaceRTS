using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;

internal class CommandProcessor
{
    private Queue<Command> commands = [];

    public void QueueCommand(Command command)
    {
        commands.Enqueue(command);
    }

    public void Flush()
    {
        World.TurnProcessor.SubmitCommands(World.TurnProcessor.CurrentTurn + 2, Program.Lobby!.ClientID, commands.ToArray());
        commands.Clear();
    }
}
