using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal abstract class Packet : Actor
{
    public Packet(PacketPrototype prototype) : base(prototype)
    {
    }
}

abstract class PacketPrototype : Prototype
{
    public abstract override Packet Deserialize(BinaryReader reader);
}