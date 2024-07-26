using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking.Packets;

/// <summary>
/// Sent by the client when it first connects to the server.
/// </summary>
[PacketID("helo")]
internal class HelloPacket : Packet
{
    public string PlayerName { get; set; }
}
