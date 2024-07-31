using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking.Packets;

[PacketID("wlcm")]
internal class WelcomePacket : Packet
{
    public int ClientID;
    public bool IsMaster;
    public PlayerInfo[] OtherPlayers;

    public class PlayerInfo
    {
        public PlayerInfo(int id, string name)
        {
            ID = id;
            Name = name;
        }

        public PlayerInfo()
        {

        }

        public int ID { get; set; }
        public string Name { get; set; }
    }
}