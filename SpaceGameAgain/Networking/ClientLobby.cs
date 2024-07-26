using SpaceGame.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class ClientLobby
{
    public readonly NetworkClient network;
    public Dictionary<int, string> players = [];
    public int ClientID;

    public ClientLobby(NetworkClient network)
    {
        this.network = network;

        network.SendPacket(new HelloPacket() { PlayerName = "jerry" });
    }

    public void Update()
    {
        network.Update();
        if (network.ReceivePacket(out WelcomePacket? welcomePacket))
        {
            ClientID = welcomePacket.ClientID;
            players.Add(welcomePacket.ClientID, "jerry");

            foreach (var otherPlayer in welcomePacket.OtherPlayers)
            {
                players.Add(otherPlayer.ID, otherPlayer.Name);
            }
        }
        if (network.ReceivePacket(out NewPlayerPacket? newPlayerPacket))
        {
            players.Add(newPlayerPacket.ID, newPlayerPacket.Name);
        }
    }
}
