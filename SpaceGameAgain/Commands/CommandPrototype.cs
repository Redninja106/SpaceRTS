using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
internal abstract class CommandPrototype : Prototype
{
    public string Description { get; set; } = "";

    public abstract void Issue(Unit? target, HashSet<Unit> selected, PlayerCommandProcessor processor);
    public abstract bool Applies(Unit? target, HashSet<Unit> selected);
}
