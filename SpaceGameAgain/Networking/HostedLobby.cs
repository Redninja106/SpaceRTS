using SpaceGame.Commands;
using SpaceGame.Debugging;
using SpaceGame.Networking.Packets;
using SpaceGame.Planets;
using SpaceGame.Serialization;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace SpaceGame.Networking;

class HostedLobby : Lobby
{
    public GameWorld World { get; }
    public SocketServer server;
    public Dictionary<Team, Socket> associations = [];
    private List<Socket> currentWorldDownloads = [];

    public override bool IsDownloadingWorld => currentWorldDownloads.Count > 0;

    public HostedLobby(GameWorld world, SocketServer server)
    {
        this.World = world;
        this.server = server;
    }

    public override void Update()
    {
        server.Update();
        
        if (World.idleTicks >= TurnProcessor.TicksPerTurn * TurnProcessor.TurnDelay)
        {
            foreach (var team in World.Teams)
            {
                if (!team.GetCommandProcessor().HasCommands(World.TurnProcessor.turn))
                {
                    server.Send(new TurnRequestPacket() { turn = World.TurnProcessor.turn, history = World.TurnProcessor.history }, associations[team]);
                    DebugLog.Message($"requested turn {World.TurnProcessor.turn} from team {team.ID}");
                }
            }
            World.idleTicks = 0;
        }

        if (server.ReceivePacket<HelloPacket>(out var hello, out var connection))
        {
            DebugLog.Message("got a hello from " + hello.ClientName);
            
            Planet startingPlanet = Random.Shared.GetItems(World.Planets.ToArray(), 1).Single();

            CreateTeamPacket createTeamPacket = new CreateTeamPacket()
            {
                name = hello.ClientName,
                teamPrototype = Prototypes.Get<TeamPrototype>("player_team"),
                money = 1000
            };


            // Team team = new Team(Prototypes.Get<TeamPrototype>("team"), World.NewID(), Transform.Default);
            // team.Money = 1000;
            // associations[team] = connection;
            // team.CommandProcessor = new NetworkCommandProcessor();

            // Ship startingShip = new Ship(Prototypes.Get<ShipPrototype>("small_ship"), World.NewID(), startingPlanet.Transform, team.AsReference());
            // ConstructionModule constMod = new ConstructionModule(Prototypes.Get<ConstructionModulePrototype>("construction_module"), World.NewID(), startingShip.AsReference());
            // startingShip.modules.Add(constMod.AsReference<Module>());

            // World.Add(team);
            // World.Add(startingShip);
            // World.Add(constMod);

            server.SendAll(createTeamPacket, s => s != connection);

            Team team = createTeamPacket.CreateTeam(World);
            DebugLog.Message(hello.ClientName + "'s team is " + team.ID);
            associations[team] = connection;
            World.Add(team);

            SendWorld(connection, team);

            SummonShipCommand summonShipCommand = new SummonShipCommand() 
            {
                team = team,
                shipPrototype = Prototypes.Get<ShipPrototype>("small_ship"),
                modulePrototypes = [Prototypes.Get<ModulePrototype>("construction_module")],
                transform = startingPlanet.Transform
            };

            ((PlayerCommandProcessor)World.PlayerTeam.GetCommandProcessor()).AddCommand(summonShipCommand);
        }

        if (server.ReceivePacket<WorldDownloadCompletePacket>(out var _, out connection))
        {
            currentWorldDownloads.Remove(connection);
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
        // var prototype = Prototypes.Get<WorldDownloadPacketPrototype>("world_download_packet");
        
        WorldSerializer serializer = new();
        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        serializer.Serialize(Program.World, Program.SerializationContext, writer);

        byte[] data = ms.GetBuffer();
        int position = 0, index = 0;
        List<WorldDataPacket> chunks = [];
        while (position < data.Length)
        {
            int length = Math.Min(data.Length - position, WorldDataPacket.ChunkSize);
            byte[] chunkData = data.AsSpan(position, length).ToArray();
            chunks.Add(new WorldDataPacket() {  packetIndex = index, data = chunkData });
            
            position += WorldDataPacket.ChunkSize;
            index++;
        }

        WorldDownloadPacket worldPacket = new() { teamIDToPlayAs = teamToPlayAs.ID, numberOfChunks = chunks.Count };

        World.TurnProcessor.startingTurn = World.TurnProcessor.turn;
        server.Send(worldPacket, connection);

        for (int i = 0; i < chunks.Count; i++)
        {
            server.Send(chunks[i], connection);
        }

        currentWorldDownloads.Add(connection);
    }
}
