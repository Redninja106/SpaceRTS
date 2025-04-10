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
    public int numberOfChunks;

    public WorldDownloadPacket(WorldDownloadPacketPrototype prototype, ActorReference<Team> team, int numberOfChunks) : base(prototype)
    {
        this.teamToPlayAs = team;
        this.numberOfChunks = numberOfChunks;
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(teamToPlayAs);
        writer.Write(numberOfChunks);
    }
}

class WorldDownloadPacketPrototype : PacketPrototype
{
    public override Packet Deserialize(BinaryReader reader)
    {
        ActorReference<Team> team = reader.ReadActorReference<Team>();
        int numberOfChunks = reader.ReadInt32();

        return new WorldDownloadPacket(this, team, numberOfChunks);
    }
}
