
using SpaceGame.Networking.Packets;

namespace SpaceGame.Networking;

[Serializable]
internal class TurnRequestPacket : Packet
{
    [Serialize]
    public ulong turn;
    [Serialize]
    public TurnHistory history;
}