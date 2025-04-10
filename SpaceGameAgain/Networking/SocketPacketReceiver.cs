using System.Net.Sockets;

namespace SpaceGame.Networking;

class SocketPacketReceiver
{
    private Socket socket;
    private RingBuffer ringBuffer;
    private BinaryReader reader;

    public SocketPacketReceiver(Socket socket)
    {
        this.socket = socket;
        ringBuffer = new RingBuffer(NetworkSettings.MaxPacketSize);
        reader = new(ringBuffer);
    }

    public List<Packet> ReceivePackets()
    {
        List<Packet> packets = [];
        byte[] recvbuf = new byte[1024];
        while (socket.Poll(10, SelectMode.SelectRead))
        {
            int received = socket.Receive(recvbuf);
            ringBuffer.Write(recvbuf.AsSpan(0, received));

            while (ringBuffer.PeakInt32() < ringBuffer.Length)
            {
                byte[]? readBuf = null;
                if (NetworkSettings.LogIncomingPackets)
                {
                    int f = ringBuffer.Front, b = ringBuffer.Back;
                    readBuf = new byte[ringBuffer.PeakInt32()];
                    ringBuffer.Read(readBuf.AsSpan());
                    ringBuffer.Front = f;
                    ringBuffer.Back = b;
                }

                int packetSize = reader.ReadInt32();
                Packet packet = (Packet)Program.NetworkSerializer.Deserialize(reader);
                packets.Add(packet);

                if (NetworkSettings.LogIncomingPackets)
                {
                    DebugLog.Message($"got packet {packet.Prototype.Name}, data [{string.Join(',', readBuf)}]");
                }
            }
        }
        return packets;
    }

}
