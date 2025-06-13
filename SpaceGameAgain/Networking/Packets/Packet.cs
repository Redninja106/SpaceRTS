using SpaceGame.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking.Packets;

[Serializable(Abstract = true)]
internal abstract class Packet
{
}

//abstract class PacketPrototype : Prototype
//{
//    public abstract override Packet Deserialize(BinaryReader reader);
//}