using SpaceGame.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal abstract class NetworkHost
{
    public event Action<int> PlayerConnected;
    public event Action<int> PlayerDisconnected;

    public abstract void SendPacket(int playerID, Packet packet);
    public abstract bool ReceivePacket<TPacket>([NotNullWhen(true)] out TPacket? packet, out int playerId)
        where TPacket : Packet;

    public abstract void Update();
}