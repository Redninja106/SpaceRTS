using SpaceGame.Commands;
using SpaceGame.Orders;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;

internal class TurnPacket : Packet
{
    public ulong turn;
    public ActorReference<Team> team;
    public List<Command> commands;
    public List<Command> prevTurnCommands;

    public TurnPacket(TurnPacketPrototype prototype, ulong turn, ActorReference<Team> team, List<Command> commands, List<Command> prevTurnCommands) : base(prototype)
    {
        this.turn = turn;
        this.team = team;
        this.commands = commands;
        this.prevTurnCommands = prevTurnCommands;
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(turn);
        writer.Write(team);

        writer.Write(commands.Count);
        for (int i = 0; i < commands.Count; i++)
        {
            Program.NetworkSerializer.Serialize(commands[i], writer);
        }
        writer.Write(prevTurnCommands.Count);
        for (int i = 0; i < prevTurnCommands.Count; i++)
        {
            Program.NetworkSerializer.Serialize(prevTurnCommands[i], writer);
        }
    }

    public void Process()
    {
        if (team.Actor!.CommandProcessor is not NetworkCommandProcessor commandProcessor)
        {
            // Debug.Assert(false);
            DebugLog.Warning($"Received commands for player controlled team {team.ID}. Dropping commands.");
            return;
        }

        if (this.turn < World.TurnProcessor.turn)
        {
            return;
        }

        if (!commandProcessor.HasCommands(turn))
        {
            commandProcessor.AddCommands(turn, commands.ToArray());
        }
        else
        {
            DebugLog.Warning($"received turn {turn} for {team.Actor!} twice!");
        }

        if (World.TurnProcessor.turn < turn && !commandProcessor.HasCommands(turn - 1))
        {
            commandProcessor.AddCommands(turn - 1, prevTurnCommands.ToArray());
        }

        // Console.WriteLine("got commands for turn " + turn);
    }
}

class TurnPacketPrototype : PacketPrototype
{
    public override Packet Deserialize(BinaryReader reader)
    {
        ulong turn = reader.ReadUInt64();
        ActorReference<Team> team = reader.ReadActorReference<Team>();

        List<Command> commands = [];
        int commandCount = reader.ReadInt32();
        for (int i = 0; i < commandCount; i++)
        {
            Command command = (Command)Program.NetworkSerializer.Deserialize(reader);
            commands.Add(command);
        }

        List<Command> prevTurnCommands = [];
        int prevCommandCount = reader.ReadInt32();
        for (int i = 0; i < prevCommandCount; i++)
        {
            Command command = (Command)Program.NetworkSerializer.Deserialize(reader);
            prevTurnCommands.Add(command);
        }

        return new TurnPacket(this, turn, team, commands, prevTurnCommands);
    }
}
