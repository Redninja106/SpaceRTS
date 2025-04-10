using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;

internal class SocketClient
{
    private Socket connection;
    private List<Packet> receivedPackets = [];
    private SocketPacketReceiver packetReceiver;

    public SocketClient(string host, int port)
    {
        connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, 0);
        connection.Blocking = false;
        connection.NoDelay = true;

        Task connectTask = connection.ConnectAsync(host, port);

        connectTask.GetAwaiter().GetResult();

        DebugLog.Message("connected to " + connection.RemoteEndPoint?.ToString());

        packetReceiver = new(connection);
    }

    public void Update()
    {
        receivedPackets.AddRange(packetReceiver.ReceivePackets());
    }

    public void SendPacket(Packet packet)
    {
        byte[] packetData = Program.NetworkSerializer.SerializeWithLengthPrefix(packet);
        connection.Send(packetData);
        if (NetworkSettings.LogOutgoingPackets)
        {
            DebugLog.Message($"sent packet {packet.Prototype.Name}, data [{string.Join(',', packetData)}]");
        }
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
