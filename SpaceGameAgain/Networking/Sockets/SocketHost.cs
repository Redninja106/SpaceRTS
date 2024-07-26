using SpaceGame.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NVorbis.Contracts;

namespace SpaceGame.Networking.Sockets;
internal class SocketHost : NetworkHost
{
    private Socket listeningSocket;
    private Dictionary<Socket, int> socketsToIds;
    private Dictionary<int, Socket> idsToSockets;
    private int nextPlayerId = 1;
    private Dictionary<Type, Queue<(Packet, int)>> receivedPackets = [];


    public SocketHost(int port = NetworkSettings.DefaultPort)
    {
        listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, 0);
        listeningSocket.Bind(new IPEndPoint(new IPAddress([127, 0, 0, 1]), port));
        listeningSocket.Listen();

        socketsToIds = [];
        idsToSockets = [];
    }

    public override void Update()
    {
        byte[] buffer = new byte[NetworkSettings.MaxPacketSize];

        List<Socket> checkRead = [listeningSocket, .. socketsToIds.Keys];

        if (checkRead.Count > 0)
        {
            Socket.Select(checkRead, null, null, 10);
            foreach (var socket in checkRead)
            {
                if (socket == listeningSocket)
                {
                    var connection = listeningSocket.Accept();
                    connection.Blocking = false;
                    int id = nextPlayerId++;
                    socketsToIds.Add(connection, id);
                    idsToSockets.Add(id, connection);
                    continue;
                }

                int received = socket.Receive(buffer);
                Span<byte> bytes = buffer.AsSpan(0, received);
                using var ms = new MemoryStream(buffer, false);
                var packet = Packet.Deserialize(ms);

                OnPacketReceived(packet, socketsToIds[socket]);
            }
        }
    }

    private void OnPacketReceived(Packet packet, int playerId)
    {
        if (!receivedPackets.TryGetValue(packet.GetType(), out var queue))
        {
            queue = [];
            receivedPackets[packet.GetType()] = queue;
        }

        queue.Enqueue((packet, playerId));
    }

    public override void SendPacket(int playerID, Packet packet)
    {
        using MemoryStream memoryStream = new();
        Packet.Serialize(packet, memoryStream);
        idsToSockets[playerID].Send(memoryStream.GetBuffer());
    }

    public override bool ReceivePacket<TPacket>([NotNullWhen(true)] out TPacket? packet, out int playerId)
        where TPacket : class
    {
        if (receivedPackets.TryGetValue(typeof(TPacket), out var value))
        {
            if (value.Count > 0)
            {
                var (pk, pr) = value.Dequeue();
                packet = (TPacket)pk;
                playerId = pr;
                return true;
            }
        }

        packet = null;
        playerId = 0;

        return false;
    }
}
