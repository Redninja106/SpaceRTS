using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class WorldSerializer
{
    public GameWorld Deserialize(BinaryReader reader)
    {
        var world = World = new();

        world.NextID = reader.ReadUInt64();
        world.PlayerTeam = reader.ReadActorReference<Team>();
        world.TurnProcessor.startingTurn = world.TurnProcessor.turn = reader.ReadUInt64();
        world.TurnProcessor.RemainingTicks = reader.ReadInt32();
        world.tick = reader.ReadUInt64();

        int prototypeCount = reader.ReadInt32();

        for (int i = 0; i < prototypeCount; i++)
        {
            string prototypeName = reader.ReadString();
            int actorCount = reader.ReadInt32();

            WorldActorPrototype prototype = Prototypes.Get<WorldActorPrototype>(prototypeName);

            for (int j = 0; j < actorCount; j++)
            {
                world.Add(prototype.Deserialize(reader)); 
                
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

        foreach (var (id, actor) in world.Actors)
        {
            actor.FinalizeDeserialization();
        }

        return world;
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
