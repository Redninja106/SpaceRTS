using Silk.NET.OpenGL;
using SpaceGame.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking.Sockets;
internal class SocketClient : NetworkClient
{
    private Socket connection;
    private Dictionary<Type, Queue<Packet>> receivedPackets = [];

    public SocketClient(string host, int port)
    {
        connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, 0);
        connection.Connect(host, port);
        connection.Blocking = false;
    }

    public override bool ReceivePacket<TPacket>([NotNullWhen(true)] out TPacket? packet) 
        where TPacket : class
    {
        if (receivedPackets.TryGetValue(typeof(TPacket), out var value))
        {
            if (value.Count > 0)
            {
                packet = (TPacket)value.Dequeue();
                return true;
            }
        }

        packet = null;

        return false;
    }

    public override void SendPacket(Packet packet)
    {
        using MemoryStream memoryStream = new();
        Packet.Serialize(packet, memoryStream);
        connection.Send(memoryStream.GetBuffer());
    }

    public override void Update()
    {
        List<Socket> sockets = [connection];
        Socket.Select(sockets, null, null, 100);

        byte[] buffer = new byte[NetworkSettings.MaxPacketSize];
        foreach (var socket in sockets)
        {
            int received = socket.Receive(buffer);
            Span<byte> bytes = buffer.AsSpan(0, received);
            using var ms = new MemoryStream(buffer, false);
            var packet = Packet.Deserialize(ms);

            OnPacketReceived(packet);
        }
    }

    private void OnPacketReceived(Packet packet)
    {
        if (!receivedPackets.TryGetValue(packet.GetType(), out var queue))
        {
            queue = [];
            receivedPackets[packet.GetType()] = queue;
        }

        queue.Enqueue(packet);
    }
}
