﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Teams;
internal class TeamPrototype : Prototype
{
    public override Actor? Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();

        int credits = reader.ReadInt32();

        int relationshipCount = reader.ReadInt32();

        Dictionary<ActorReference<Team>, TeamRelation> relationships = [];
        for (int i = 0; i < relationshipCount; i++)
        {
            ActorReference<Team> team = reader.ReadActorReference<Team>();
            TeamRelation relation = (TeamRelation)reader.ReadInt32();

            relationships.Add(team, relation);
        }

        return new Team(this, id, Transform.Default, credits, relationships);
    }
}
