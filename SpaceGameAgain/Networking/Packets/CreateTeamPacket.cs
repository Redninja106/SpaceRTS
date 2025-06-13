using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking.Packets;

[Serializable]
internal class CreateTeamPacket : Packet
{
    [Serialize]
    public required string name;
    [Serialize]
    public required TeamPrototype teamPrototype;
    [Serialize]
    public required int money;

    public Team CreateTeam()
    {
        return new Team(teamPrototype, World.NewID())
        {
            Money = money,
            Name = name
        };
    }
}
