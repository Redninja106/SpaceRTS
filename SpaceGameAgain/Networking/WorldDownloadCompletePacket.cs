using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class WorldDownloadCompletePacket : Packet
{
    public WorldDownloadCompletePacket(PacketPrototype prototype) : base(prototype)
    {
    }

    public override void Serialize(BinaryWriter writer)
    {
    }
}

internal class WorldDownloadCompletePacketPrototype : PacketPrototype
{
    public override Packet Deserialize(BinaryReader reader)
    {
        return new WorldDownloadCompletePacket(this);
    }
}
