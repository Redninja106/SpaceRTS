using SpaceGame.Commands;

namespace SpaceGame.Networking;

class NetworkLobby : Lobby
{
    public NetworkClient network;

    public NetworkLobby(NetworkClient network)
    {
        this.network = network;

        network.SendPacket(new HelloPacket(Prototypes.Get<HelloPacketPrototype>("hello_packet"), "jerry"));
    }

    public override void Update()
    {
        network.Update();

        if (network.ReceivePacket(out WorldDownloadPacket? worldDownload))
        {
            WorldSerializer serializer = new();

            using var ms = new MemoryStream(worldDownload.data);
            using var reader = new BinaryReader(ms);
            World = serializer.Deserialize(reader);
            World.PlayerTeam = worldDownload.teamToPlayAs;
            foreach (var team in World.Teams)
            {
                if (team == worldDownload.teamToPlayAs.Actor) 
                {
                    team.CommandProcessor = new PlayerCommandProcessor();
                }
                else 
                {
                    team.CommandProcessor = new NetworkCommandProcessor();
                }
            }

            Console.WriteLine("downloaded world!");
        }

        if (network.ReceivePacket(out TurnRequestPacket? turnRequest))
        {
            Console.WriteLine($"got a request for turn {turnRequest.turn}");

            var cmdProc = (PlayerCommandProcessor)World.PlayerTeam.Actor!.CommandProcessor;
            for (ulong t = World.TurnProcessor.turn; t < turnRequest.turn; t++)
            {
                var turnHistory = turnRequest.history.GetTurn(t);
                foreach (var (team, commands) in turnHistory)
                {
                    if (team.CommandProcessor is NetworkCommandProcessor proc && !proc.HasCommands(t))
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
            //    //var commands = turnDict[World.PlayerTeam.Actor!];

            //}
        }

        if (network.ReceivePacket(out TurnPacket? turn))
        {
            turn.Process();
        }
    }
}
