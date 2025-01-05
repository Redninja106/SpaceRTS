
namespace SpaceGame.Networking;

internal class TurnRequestPacket : Packet
{
    public ulong turn;
    public TurnHistory history;

    public TurnRequestPacket(TurnRequestPacketPrototype prototype, ulong turn, TurnHistory history) : base(prototype)
    {
        this.turn = turn;
        this.history = history;
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(turn);
        history.Serialize(writer);
    }
}
class TurnRequestPacketPrototype : PacketPrototype
{
    public override Packet Deserialize(BinaryReader reader)
    {
        ulong turn = reader.ReadUInt64();
        TurnHistory history = TurnHistory.Deserialize(reader);

        return new TurnRequestPacket(this, turn, history);
    }
}