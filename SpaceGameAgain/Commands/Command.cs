using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;

[Serializable(Abstract = true)]
internal abstract class Command
{
    public abstract void Apply();
}
