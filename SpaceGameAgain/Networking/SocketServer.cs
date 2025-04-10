using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class SocketServer
{
    private Socket listeningSocket;
    private List<SocketClientInterface> clients = [];
    // private Dictionary<Socket, int> socketToClientID = [];
    // private Dictionary<int, Socket> clientIDToSocket = [];
    // private int nextClientId = 1;

    [DebugOverlay]
    public static bool LogServerPackets = false;

    public SocketServer(int port)
    {
        listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, 0);
        listeningSocket.Bind(new IPEndPoint(IPAddress.Any, port));
        listeningSocket.Listen();
        listeningSocket.NoDelay = true;
        DebugLog.Message("listening on port " + port.ToString());
    }

    public void Update()
    {
        if (listeningSocket.Poll(10, SelectMode.SelectRead))
        {
            var client = listeningSocket.Accept();
            client.NoDelay = true;
            clients.Add(new SocketClientInterface(client));

            DebugLog.Message("accepted connection from " + client.RemoteEndPoint!.ToString());
        }

        foreach (var client in clients)
        {
            client.Update();
        }
    }

    //private void ReadClientPackets(Socket client)
    //{
    //    byte[] buffer = new byte[1024 * 64];
    //    try
    //    {
    //        if (client.Poll(10, SelectMode.SelectRead))
    //        {
    //            int received = client.Receive(buffer);
    //            using MemoryStream ms = new(buffer);
    //            using BinaryReader reader = new(ms);
    //            int packetSize = reader.ReadInt32();
    //            Packet packet = (Packet)Program.NetworkSerializer.Deserialize(reader);
    //            receivedPackets.Add((packet, client));
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        DebugLog.Message(ex.ToString());
    //    }
    //}

    public bool ReceivePacket<TPacket>([NotNullWhen(true)] out TPacket? packet, [NotNullWhen(true)] out Socket? client)
        where TPacket : Packet
    {
        foreach (var c in clients)
        {
            for (int i = 0; i < c.receivedPackets.Count; i++)
            {
                if (c.receivedPackets[i] is TPacket p)
                {
                    packet = p;
                    client = c.socket;
                    c.receivedPackets.RemoveAt(i);
                    return true;
                }
            }
        }


        packet = null;
        client = null;
        return false;
    }

    public void SendAll(Packet packet)
    {
        byte[] data = Program.NetworkSerializer.SerializeWithLengthPrefix(packet);

        foreach (var client in clients)
        {
            client.socket.Send(data);
        }
    }

    public void Send(Packet packet, Socket client)
    {
        byte[] data = Program.NetworkSerializer.SerializeWithLengthPrefix(packet);

        if (LogServerPackets)
        {
            DebugLog.Message($"sent {data.Length} byte {packet.Prototype.Name} to {client.RemoteEndPoint}");
        }

        client.Send(data);
    }

    class SocketClientInterface
    {
        public Socket socket;
        public SocketPacketReceiver packetReceiver;
        public List<Packet> receivedPackets = [];

        public SocketClientInterface(Socket socket)
        {
            this.socket = socket;
            this.packetReceiver = new(socket);
        }

        public void Update()
        {
            this.receivedPackets.AddRange(packetReceiver.ReceivePackets());
        }
    }
}
