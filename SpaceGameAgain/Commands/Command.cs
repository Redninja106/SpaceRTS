using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
internal abstract class Command(CommandPrototype prototype) : Actor(prototype)
{
    public abstract void Apply();
}
