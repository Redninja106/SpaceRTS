using SpaceGame.Debugging;
using SpaceGame.Networking.Packets;
using System.Diagnostics;
using System.Net.Sockets;

namespace SpaceGame.Networking;

class SocketPacketReceiver
{
    private Socket socket;
    private RingBuffer ringBuffer;
    private BinaryReader reader;
    private Serializer packetSerializer;

    public SocketPacketReceiver(Socket socket)
    {
        this.socket = socket;
        ringBuffer = new RingBuffer(NetworkSettings.MaxPacketSize);
        reader = new(ringBuffer);
        packetSerializer = Serializer.GetSerializer(typeof(Packet));
    }

    // reads all packets we've fully received
    // leaves excess data on the ring buffer for next call to this method
    public List<Packet> ReceivePackets()
    {
        List<Packet> packets = [];
        byte[] recvbuf = new byte[1024];
        while (socket.Poll(10, SelectMode.SelectRead))
        {
            // read bytes into ring buffer
            int received = socket.Receive(recvbuf);
            ringBuffer.Write(recvbuf.AsSpan(0, received));

            // peek the packet size (it was written using SerializeWithLengthPrefix) and all the complete packets in the buffer
            int packetSize = ringBuffer.PeakInt32();
            while (ringBuffer.Length >= 4 && packetSize <= ringBuffer.Length)
            {
                long prevLength = ringBuffer.Length;
                // consume packet size (it was only peeked before)
                _ = reader.ReadInt32();

                // actually deserialize packet
                Packet packet = (Packet)packetSerializer.Deserialize(reader);
                packets.Add(packet);

                DebugLog.Assert(packetSize == prevLength - ringBuffer.Length);
                if (NetworkSettings.LogIncomingPackets)
                {
                    DebugLog.Message($"got packet {packet.GetType().Name}");
                }

                // peek length of next packed if available
                if (ringBuffer.Length >= 4)
                {
                    packetSize = ringBuffer.PeakInt32();
                }
            }

        }
        return packets;
    }


    public static byte[] SerializeWithLengthPrefix(Packet packet, Serializer packetSerializer)
    {
        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        writer.Write(0); // leave 4 bytes for packet size

        packetSerializer.Serialize(writer, packet);

        // fill in those 4 bytes
        // NOTE the length includes the 4 bytes for itself
        int position = (int)ms.Position;
        ms.Position = 0;
        writer.Write(position);

        return ms.ToArray();
    }
}
