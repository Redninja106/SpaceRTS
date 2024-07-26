using SpaceGame.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking.Packets;

[PacketID("cmds")]
internal class CommandListPacket : Packet
{
    public long turn;
    public int playerId;
    public Command[] Commands;
}
