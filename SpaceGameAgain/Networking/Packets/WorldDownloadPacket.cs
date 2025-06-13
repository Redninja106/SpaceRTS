using SpaceGame.Extensions;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking.Packets;

[Serializable]
internal class WorldDownloadPacket : Packet
{
    [Serialize]
    public required ulong teamIDToPlayAs;
    [Serialize]
    public required int numberOfChunks;
}