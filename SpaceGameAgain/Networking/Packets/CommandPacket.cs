using SpaceGame.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking.Packets;
internal class CommandPacket
{
    public ulong turn;
    public Command command;
}
