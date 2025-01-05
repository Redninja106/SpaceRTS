using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
internal interface ICommandProcessor
{
    bool HasCommands(ulong turn);

    Command[] GetCommands(ulong turn);
    void RemoveCommands(ulong turn);
}
