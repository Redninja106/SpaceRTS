using SpaceGame.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class HostLobby
{
    public readonly NetworkHost network;
    private Dictionary<int, string> players = [];

    public HostLobby(NetworkHost network)
    {
        this.network = network;
        network.PlayerConnected += Network_PlayerConnected;
        network.PlayerDisconnected += Network_PlayerDisconnected;
    }

    private void Network_PlayerDisconnected(int playerId)
    {
    }

    private void Network_PlayerConnected(int playerId)
    {
    }

    public void Update()
    {
        network.Update();
        if (network.ReceivePacket(out HelloPacket? packet, out int playerId))
        {
            network.SendPacket(playerId, new WelcomePacket()
            {
                ClientID = playerId,
                OtherPlayers = players.Select(kv => new WelcomePacket.PlayerInfo(kv.Key, kv.Value)).ToArray()
            });

            NewPlayerPacket newPlayerPacket = new NewPlayerPacket()
            {
                ID = playerId,
                Name = packet.PlayerName,
            };
            foreach (var (id, name) in players)
            {
                network.SendPacket(id, newPlayerPacket);
            }

            players.Add(playerId, packet.PlayerName);
        }

    }

    public void StartGame()
    {
        foreach (var id in players.Keys)
        {
            network.SendPacket(id, new StartGamePacket());
        }
    }
}
