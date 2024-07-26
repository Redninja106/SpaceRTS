using SpaceGame.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;

[PacketBaseClass]
internal abstract class Command
{
    public abstract void Apply();
}
