using SpaceGame.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal abstract class NetworkClient
{
    public abstract void SendPacket(Packet packet);
    public abstract bool ReceivePacket<TPacket>([NotNullWhen(true)] out TPacket? packet)
        where TPacket : Packet;

    public abstract void Update();
}
