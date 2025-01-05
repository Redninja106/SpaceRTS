using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class WorldDownloadPacket : Packet
{
    public ActorReference<Team> teamToPlayAs;
    public byte[] data;

    public WorldDownloadPacket(WorldDownloadPacketPrototype prototype, ActorReference<Team> team, byte[] data) : base(prototype)
    {
        this.teamToPlayAs = team;
        this.data = data;
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(teamToPlayAs);
        writer.Write(data.Length);
        writer.Write(data);
    }
}

class WorldDownloadPacketPrototype : PacketPrototype
{
    public override Packet Deserialize(BinaryReader reader)
    {
        ActorReference<Team> team = reader.ReadActorReference<Team>();
        int byteCount = reader.ReadInt32();
        byte[] data = reader.ReadBytes(byteCount);

        return new WorldDownloadPacket(this, team, data);
    }
}
