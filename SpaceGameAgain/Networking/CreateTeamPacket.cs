using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class CreateTeamPacket : Packet
{
    private string name;
    private TeamPrototype teamPrototype;
    private int money;

    public CreateTeamPacket(CreateTeamPacketPrototype prototype, string name, TeamPrototype teamPrototype, int money) : base(prototype)
    {
        this.name = name;
        this.teamPrototype = teamPrototype;
        this.money = money;
    }

    public Team CreateTeam()
    {
        return new Team(teamPrototype, World.NewID(), Transform.Default, money: money, name: name);
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(name);
        writer.Write(teamPrototype.Name);
        writer.Write(money);
    }
}

class CreateTeamPacketPrototype : PacketPrototype
{
    public override CreateTeamPacket Deserialize(BinaryReader reader)
    {
        string name = reader.ReadString();
        TeamPrototype prototype = Prototypes.Get<TeamPrototype>(reader.ReadString());
        int money = reader.ReadInt32();
        return new CreateTeamPacket(this, name, prototype, money);
    }
}
