using SpaceGame.Commands;
using SpaceGame.Extensions;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Serialization;
internal class WorldSerializer
{
    public GameWorld Deserialize(BinaryReader reader)
    {
        GameWorld world = new();
        SerializationContext serializationContext = new(world);
        // fieldSerializer.Deserialize(reader, world);

        world.NextID = reader.ReadUInt64();
        ulong playerTeamId = reader.ReadUInt64();
        world.TurnProcessor.startingTurn = world.TurnProcessor.turn = reader.ReadUInt64();
        world.TurnProcessor.RemainingTicks = reader.ReadInt32();
        world.tick = reader.ReadUInt64();

        int prototypeCount = reader.ReadInt32();
        ActorDeserializationList[] actorLists = new ActorDeserializationList[prototypeCount];
        for (int i = 0; i < prototypeCount; i++)
        {
            actorLists[i] = new ActorDeserializationList(serializationContext, reader, world);
        }

        for (int i = 0; i < prototypeCount; i++)
        {
            actorLists[i].Deserialize(reader);
        }

        for (int i = 0; i < prototypeCount; i++)
        {
            actorLists[i].FinishDeserialization();
        }

        world.PlayerTeam = (Team)world.Actors[playerTeamId];

        return world;
    }

    public void Serialize(GameWorld world, SerializationContext context, BinaryWriter writer)
    {
        // fieldSerializer.Serialize(writer, world);

        writer.Write(world.NextID);
        writer.Write(world.PlayerTeam.ID);
        writer.Write(world.TurnProcessor.turn);
        writer.Write(world.TurnProcessor.RemainingTicks);
        writer.Write(world.tick);

        Prototype[] prototypes = Prototypes.RegisteredPrototypes.OfType<Prototype>().Where(p => world.GetActorsByPrototype(p).Any()).ToArray();
        writer.Write(prototypes.Length);

        ActorSerializationList[] serializationLists = new ActorSerializationList[prototypes.Length];
        for (int i = 0; i < prototypes.Length; i++)
        {
            serializationLists[i] = new(context, prototypes[i], world.GetActorsByPrototype(prototypes[i]).ToArray());
        }

        for (int i = 0; i < serializationLists.Length; i++)
        {
            serializationLists[i].SerializeIDs(writer);
        }

        for (int i = 0; i < serializationLists.Length; i++)
        {
            serializationLists[i].SerializeFields(writer);
        }

        //foreach (var prototype in prototypes)
        //{
        //    Actor[] actors = world.GetActorsByPrototype(prototype).ToArray();
        //    ActorSerializer serializer = new ActorSerializer(prototype);

        //    writer.Write(prototype.Name);
        //    writer.Write(actors.Length);

        //    foreach (Actor actor in actors)
        //    {
        //        serializer.Serialize(writer, actor);
        //        // actor.Serialize(writer);
        //        writer.Write(0);
        //    }

        //    writer.Write(0);
        //}


        //writer.Write(prototypes.Length);

    }

}
