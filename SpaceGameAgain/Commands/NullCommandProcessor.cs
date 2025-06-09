using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
internal class NullCommandProcessor : ICommandProcessor
{
    public Command[] GetCommands(ulong turn)
    {
        return [];
    }

    public bool HasCommands(ulong turn)
    {
        return true;
    }

    public void RemoveCommands(ulong turn)
    {
    }
}
