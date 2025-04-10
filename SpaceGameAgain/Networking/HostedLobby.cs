using SpaceGame.Commands;
using SpaceGame.Planets;
using SpaceGame.Ships;
using SpaceGame.Teams;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace SpaceGame.Networking;

class HostedLobby : Lobby
{
    public SocketServer server;
    public Dictionary<Team, Socket> associations = [];

    public HostedLobby(SocketServer server)
    {
        this.server = server;
    }

    public override void Update()
    {
        server.Update();
        
        if (World.idleTicks >= TurnProcessor.TicksPerTurn * TurnProcessor.TurnDelay)
        {
            foreach (var team in World.Teams)
            {
                if (!team.CommandProcessor.HasCommands(World.TurnProcessor.turn))
                {
                    var prototype = Prototypes.Get<TurnRequestPacketPrototype>("turn_request");
                    server.Send(new TurnRequestPacket(prototype, World.TurnProcessor.turn, World.TurnProcessor.history), associations[team]);
                    DebugLog.Message($"requested turn {World.TurnProcessor.turn} from team {team.ID}");
                }
            }
            World.idleTicks = 0;
        }

        if (server.ReceivePacket<HelloPacket>(out var hello, out var connection))
        {
            DebugLog.Message("got a hello from " + hello.ClientName);
            
            Planet startingPlanet = (Planet)Random.Shared.GetItems(World.GetActorsByPrototype(Prototypes.Get<PlanetPrototype>("generic_planet")).ToArray(), 1).Single();

            Team team = new Team(Prototypes.Get<TeamPrototype>("team"), World.NewID(), Transform.Default);
            associations[team] = connection;
            team.CommandProcessor = new NetworkCommandProcessor();

            Ship startingShip = new Ship(Prototypes.Get<ShipPrototype>("small_ship"), World.NewID(), startingPlanet.Transform, team.AsReference());

            World.Add(team);
            World.Add(startingShip);

            SendWorld(connection, team);
        }

        if (server.ReceivePacket<TurnPacket>(out var turn, out connection))
        {
            foreach (var (_, c) in associations)
            {
                if (c != connection)
                {
                    server.Send(turn, c);
                }
            }

            turn.Process();
        }
    }

    private void SendWorld(Socket connection, Team teamToPlayAs)
    {
        DebugLog.Message("sending world to " + (connection.RemoteEndPoint!.ToString()));
        var prototype = Prototypes.Get<WorldDownloadPacketPrototype>("world_download_packet");
        WorldSerializer serializer = new();
        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        serializer.Serialize(World, writer);

        byte[] data = ms.GetBuffer();
        int position = 0, index = 0;
        List<WorldDataPacket> chunks = [];
        while (position < data.Length)
        {
            int length = Math.Min(data.Length - position, WorldDataPacket.ChunkSize);
            byte[] chunkData = data.AsSpan(position, length).ToArray();
            chunks.Add(new WorldDataPacket(Prototypes.Get<WorldDataPacketPrototype>("world_data_packet"), index, chunkData));
            
            position += WorldDataPacket.ChunkSize;
            index++;
        }

        WorldDownloadPacket worldPacket = new(prototype, teamToPlayAs.AsReference(), chunks.Count);

        World.TurnProcessor.startingTurn = World.TurnProcessor.turn;
        server.Send(worldPacket, connection);

        for (int i = 0; i < chunks.Count; i++)
        {
            server.Send(chunks[i], connection);
        }
    }
}
