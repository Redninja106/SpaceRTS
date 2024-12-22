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
        int prototypeCount = reader.ReadInt32();

        for (int i = 0; i < prototypeCount; i++)
        {
            string prototypeName = reader.ReadString();
            int actorCount = reader.ReadInt32();

            Prototype prototype = Prototypes.Get(prototypeName);

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

        return world;
    }

    public void Serialize(GameWorld world, BinaryWriter writer)
    {
        Prototype[] prototypes = Prototypes.RegisteredPrototypes.ToArray();

        writer.Write(world.NextID);
        writer.Write(world.PlayerTeam);
        writer.Write(prototypes.Length);

        foreach (var prototype in prototypes)
        {
            Actor[] actors = world.GetActorsByPrototype(prototype).ToArray();

            writer.Write(prototype.Name);
            writer.Write(actors.Length);

            foreach (Actor actor in actors)
            {
                actor.Serialize(writer);
                writer.Write(0);
            }

            writer.Write(0);
        }
    }
}
