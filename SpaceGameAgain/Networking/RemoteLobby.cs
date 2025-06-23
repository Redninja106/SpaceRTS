using SpaceGame.Commands;
using SpaceGame.Debugging;
using SpaceGame.Networking.Packets;
using SpaceGame.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace SpaceGame.Networking;

class RemoteLobby : Lobby
{
    public GameWorld World { get; set; }
    public SocketClient client;
    private WorldDataPacket?[]? chunks;
    private WorldDownloadPacket? currentDownload;

    public override bool IsDownloadingWorld => currentDownload != null;

    public RemoteLobby(GameWorld world, SocketClient client)
    {
        this.client = client;
        this.World = world;

        client.SendPacket(new HelloPacket() {  ClientName = "jerry" });
    }

    public override void Update()
    {
        client.Update();

        if (client.ReceivePacket(out WorldDownloadPacket? worldDownload))
        {
            DebugLog.Message("starting world download");
            chunks = new WorldDataPacket?[worldDownload.numberOfChunks];
            currentDownload = worldDownload;
        }

        if (chunks != null)
        {
            // receive ALL chunks
            while (client.ReceivePacket(out WorldDataPacket? data))
            {
                DebugLog.Message("got chunk " + data.packetIndex);
                chunks[data.packetIndex] = data;
            }

            // once we have received all chunks we do the thing
            if (chunks.All(c => c != null))
            {
                // stitch into a big big array
                List<byte> combinedData = [];
                foreach (var chunk in chunks)
                {
                    combinedData.AddRange(chunk!.data);
                }

                // finally deserialize
                WorldSerializer serializer = new();
                using var ms = new MemoryStream(combinedData.ToArray());
                using var reader = new BinaryReader(ms);
                serializer.Deserialize(reader);

                World.PlayerTeam = (Teams.Team)World.Actors[currentDownload!.teamIDToPlayAs];
                //foreach (var team in World.Teams)
                //{
                //    if (team == currentDownload.teamToPlayAs.Actor)
                //    {
                //        team.CommandProcessor = new PlayerCommandProcessor();
                //    }
                //    else
                //    {
                //        team.CommandProcessor = new NetworkCommandProcessor();
                //    }
                //}

                chunks = null;
                currentDownload = null;

                DebugLog.Message("downloaded world!");

                client.SendPacket(new WorldDownloadCompletePacket());
            }
        }

        if (client.ReceivePacket(out TurnRequestPacket? turnRequest))
        {
            DebugLog.Message($"got a request for turn {turnRequest.turn}");

            var cmdProc = (PlayerCommandProcessor)World.PlayerTeam.GetCommandProcessor();
            for (ulong t = World.TurnProcessor.turn; t < turnRequest.turn; t++)
            {
                var turnHistory = turnRequest.history.GetTurn(t);
                foreach (var (team, commands) in turnHistory)
                {
                    if (team.GetCommandProcessor() is NetworkCommandProcessor proc && !proc.HasCommands(t))
                    {
                        proc.AddCommands(t, commands);
                    }
                }
            }
            if (cmdProc.HasCommands(turnRequest.turn))
            {
                cmdProc.BroadcastCommands(turnRequest.turn);
            }
            // var turnPacket = new TurnPacket(Prototypes.Get<TurnPacketPrototype>("turn_packet"), turnRequest.turn, World.PlayerTeam, cmdProc.GetCommands(turnRequest.turn).ToList());
            // network.SendPacket(turnPacket);

            // do any catching up if necessary
            // if we are behind this should send the proper turns as long as we were behind by at least the turn delay
            
            //// if we were behind then <= the turn delay, we need to send the requested turn specifically
            //if (World.TurnProcessor.turn >= turnRequest.turn - TurnProcessor.TurnDelay)
            //{
            //    //var turnDict = World.TurnProcessor.history.GetTurn(turnRequest.turn);
            //    //var commands = turnDict[World.PlayerTeam];

            //}
        }

        if (client.ReceivePacket(out TurnPacket? turn))
        {
            turn.Process();
        }

        if (client.ReceivePacket(out CreateTeamPacket? createTeam))
        {
            World.Add(createTeam.CreateTeam(World));
        }

    }
}
