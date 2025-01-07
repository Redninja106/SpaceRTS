using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class NetworkClient
{
    private Socket connection;
    private List<Packet> receivedPackets = [];

    public NetworkClient(string host, int port)
    {
        connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, 0);
        connection.Connect(host, port);
        connection.Blocking = false;
        connection.NoDelay = true;

        Console.WriteLine("connected to " + connection.RemoteEndPoint?.ToString());
    }

    public void Update()
    {
        if (connection.Poll(10, SelectMode.SelectRead))
        {
            byte[] buffer = new byte[1024 * 64];
            int received = connection.Receive(buffer);
            using MemoryStream ms = new(buffer);
            using BinaryReader reader = new(ms);
            Packet packet = (Packet)Program.NetworkSerializer.Deserialize(reader);
            receivedPackets.Add(packet);
        }
    }

    public void SendPacket(Packet packet)
    {
        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        Program.NetworkSerializer.Serialize(packet, writer);
        connection.Send(ms.ToArray());
    }


    public bool ReceivePacket<TPacket>([NotNullWhen(true)] out TPacket? packet)
        where TPacket : Packet
    {
        for (int i = 0; i < receivedPackets.Count; i++)
        {
            if (receivedPackets[i] is TPacket p)
            {
                packet = p;
                receivedPackets.RemoveAt(i);
                return true;
            }
        }

        packet = null;
        return false;
    }

}
