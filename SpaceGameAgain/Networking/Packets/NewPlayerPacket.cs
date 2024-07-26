using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking.Packets;

[PacketID("newp")]
internal class NewPlayerPacket : Packet
{
    public int ID { get; set; }
    public string Name { get; set; }
}
