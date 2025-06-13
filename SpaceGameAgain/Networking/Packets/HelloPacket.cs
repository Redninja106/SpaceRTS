using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking.Packets;

[Serializable]
internal class HelloPacket : Packet
{
    [Serialize]
    public required string ClientName;
}
