using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class WorldDataPacket : Packet
{
    public const int ChunkSize = 32 * 1024;

    public int packetIndex;
    public byte[] data;

    public WorldDataPacket(WorldDataPacketPrototype prototype, int packetIndex, byte[] data) : base(prototype)
    {
        this.packetIndex = packetIndex;
        this.data = data;
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(packetIndex);
        writer.Write(data.Length);
        writer.Write(data);
    }
}

class WorldDataPacketPrototype : PacketPrototype
{
    public override WorldDataPacket Deserialize(BinaryReader reader)
    {
        int packetIndex = reader.ReadInt32();
        int length = reader.ReadInt32();
        byte[] bytes = reader.ReadBytes(length);
        return new WorldDataPacket(this, packetIndex, bytes);
    }
}