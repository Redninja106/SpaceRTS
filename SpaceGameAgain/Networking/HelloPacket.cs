using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class HelloPacket : Packet
{
    public string ClientName;

    public HelloPacket(HelloPacketPrototype prototype, string clientName) : base(prototype)
    {
        this.ClientName = clientName;
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ClientName);
    }
}

class HelloPacketPrototype : PacketPrototype
{
    public override Packet Deserialize(BinaryReader reader)
    {
        string clientName = reader.ReadString();

        return new HelloPacket(this, clientName);
    }
}
