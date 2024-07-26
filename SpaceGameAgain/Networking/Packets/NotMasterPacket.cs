using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking.Packets;

[PacketID("nomp")]
internal class NotMasterPacket : Packet
{
    public string MasterPeerEndpoint { get; set; }
}
