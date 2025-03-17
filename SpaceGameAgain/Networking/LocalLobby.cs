using SpaceGame.Commands;
using SpaceGame.Planets;
using SpaceGame.Ships;
using SpaceGame.Teams;
using System.Net.Sockets;

namespace SpaceGame.Networking;

class LocalLobby : Lobby
{
    public NetworkHost network;
    private Dictionary<Team, Socket> associations = [];

    public LocalLobby(NetworkHost network)
    {
        this.network = network;
    }

    public override void Update()
    {
        network.Update();
        
        if (World.idleTicks >= TurnProcessor.TicksPerTurn * TurnProcessor.TurnDelay)
        {
            foreach (var team in World.Teams)
            {
                if (!team.CommandProcessor.HasCommands(World.TurnProcessor.turn))
                {
                    var prototype = Prototypes.Get<TurnRequestPacketPrototype>("turn_request");
                    network.Send(new TurnRequestPacket(prototype, World.TurnProcessor.turn, World.TurnProcessor.history), associations[team]);
                    Console.WriteLine($"requested turn {World.TurnProcessor.turn} from team {team.ID}");
                }
            }
            World.idleTicks = 0;
        }

        if (network.ReceivePacket<HelloPacket>(out var hello, out var client))
        {
            Console.WriteLine("got a hello from " + hello.ClientName);

            Planet startingPlanet = (Planet)Random.Shared.GetItems(World.GetActorsByPrototype(Prototypes.Get<PlanetPrototype>("generic_planet")).ToArray(), 1).Single();

            Team team = new Team(Prototypes.Get<TeamPrototype>("team"), World.NewID(), Transform.Default);
            associations[team] = client;
            team.CommandProcessor = new NetworkCommandProcessor();

            Ship startingShip = new Ship(Prototypes.Get<ShipPrototype>("small_ship"), World.NewID(), startingPlanet.Transform, team.AsReference());

            World.Add(team);
            World.Add(startingShip);

            Console.WriteLine("sending world to " + hello.ClientName);
            var prototype = Prototypes.Get<WorldDownloadPacketPrototype>("world_download_packet");
            WorldSerializer serializer = new();
            using MemoryStream ms = new();
            using BinaryWriter writer = new(ms);
            serializer.Serialize(World, writer);
            WorldDownloadPacket worldPacket = new(prototype, team.AsReference(), ms.GetBuffer());
            World.TurnProcessor.startingTurn = World.TurnProcessor.turn;

            network.Send(worldPacket, client);
        }

        if (network.ReceivePacket<TurnPacket>(out var turn, out client))
        {
            foreach (var (_, c) in associations)
            {
                if (c != client)
                {
                    network.Send(turn, c);
                }
            }
            // TODO: forward to other clients

            turn.Process();
        }
    }
}
