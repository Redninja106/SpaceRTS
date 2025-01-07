using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class NetworkHost
{
    private Socket listeningSocket;
    private List<Socket> clients = [];
    private List<(Packet, Socket)> receivedPackets = [];
    // private Dictionary<Socket, int> socketToClientID = [];
    // private Dictionary<int, Socket> clientIDToSocket = [];
    // private int nextClientId = 1;

    public NetworkHost(int port)
    {
        listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, 0);
        listeningSocket.Bind(new IPEndPoint(IPAddress.Any, port));
        listeningSocket.Listen();
        listeningSocket.NoDelay = true;
        Console.WriteLine("listening on port " + port.ToString());
    }

    public void Update()
    {
        if (listeningSocket.Poll(10, SelectMode.SelectRead))
        {
            var client = listeningSocket.Accept();
            client.NoDelay = true;
            clients.Add(client);

            Console.WriteLine("accepted connection from " + client.RemoteEndPoint!.ToString());
        }

        byte[] buffer = new byte[1024 * 64];
        foreach (var client in clients)
        {
            try
            {
                if (client.Poll(10, SelectMode.SelectRead))
                {
                    int received = client.Receive(buffer);
                    using MemoryStream ms = new(buffer);
                    using BinaryReader reader = new(ms);
                    Packet packet = (Packet)Program.NetworkSerializer.Deserialize(reader);
                    receivedPackets.Add((packet, client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    public bool ReceivePacket<TPacket>([NotNullWhen(true)] out TPacket? packet, [NotNullWhen(true)] out Socket? client)
        where TPacket : Packet
    {
        for (int i = 0; i < receivedPackets.Count; i++)
        {
            if (receivedPackets[i] is (TPacket p, Socket s))
            {
                packet = p;
                client = s;
                receivedPackets.RemoveAt(i);
                return true;
            }
        }

        packet = null;
        client = null;
        return false;
    }

    public void SendAll(Packet packet)
    {
        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        Program.NetworkSerializer.Serialize(packet, writer);

        foreach (var socket in clients)
        {
            socket.Send(ms.GetBuffer());
        }
    }

    public void Send(Packet packet, Socket client)
    {
        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        Program.NetworkSerializer.Serialize(packet, writer);

        client.Send(ms.GetBuffer());
    }
}
