using SpaceGame.Commands;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class WorldSerializer
{
    public void Deserialize(BinaryReader reader)
    {
        World = new();
        World.NextID = reader.ReadUInt64();
        World.PlayerTeam = reader.ReadActorReference<Team>();
        World.TurnProcessor.startingTurn = World.TurnProcessor.turn = reader.ReadUInt64();
        World.TurnProcessor.RemainingTicks = reader.ReadInt32();
        World.tick = reader.ReadUInt64();

        int prototypeCount = reader.ReadInt32();

        for (int i = 0; i < prototypeCount; i++)
        {
            string prototypeName = reader.ReadString();
            int actorCount = reader.ReadInt32();

            WorldActorPrototype prototype = Prototypes.Get<WorldActorPrototype>(prototypeName);

            for (int j = 0; j < actorCount; j++)
            {
                World.Add(prototype.Deserialize(reader)); 
                
                if (reader.ReadInt32() != 0)
                {
                    throw new("invalid save!");
                }
            }

            if (reader.ReadInt32() != 0)
            {
                throw new("invalid save!");
            }
        }

        foreach (var (id, actor) in World.Actors)
        {
            actor.FinalizeDeserialization();
        }

        // foreach (var team in World.Teams)
        // {
        //     team.CommandProcessor = new PlayerCommandProcessor();
        // }
    }

    public void Serialize(GameWorld world, BinaryWriter writer)
    {
        WorldActorPrototype[] prototypes = Prototypes.RegisteredPrototypes.OfType<WorldActorPrototype>().ToArray();

        writer.Write(world.NextID);
        writer.Write(world.PlayerTeam);
        writer.Write(world.TurnProcessor.turn);
        writer.Write(world.TurnProcessor.RemainingTicks);
        writer.Write(world.tick);

        writer.Write(prototypes.Length);

        foreach (var prototype in prototypes)
        {
            WorldActor[] actors = world.GetActorsByPrototype(prototype).ToArray();

            writer.Write(prototype.Name);
            writer.Write(actors.Length);

            foreach (WorldActor actor in actors)
            {
                actor.Serialize(writer);
                writer.Write(0);
            }

            writer.Write(0);
        }
    }
}
