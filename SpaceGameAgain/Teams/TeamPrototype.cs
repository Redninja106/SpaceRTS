using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Teams;
internal class TeamPrototype : WorldActorPrototype
{
    public override WorldActor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();

        int relationshipCount = reader.ReadInt32();

        Dictionary<ActorReference<Team>, TeamRelation> relationships = [];
        for (int i = 0; i < relationshipCount; i++)
        {
            ActorReference<Team> team = reader.ReadActorReference<Team>();
            TeamRelation relation = (TeamRelation)reader.ReadInt32();

            relationships.Add(team, relation);
        }

        int resourceCount = reader.ReadInt32();
        Dictionary<string, int> resources = [];

        for (int i = 0; i < resourceCount; i++)
        {
            string name = reader.ReadString();
            int count = reader.ReadInt32();
            resources.Add(name, count);
        }


        return new Team(this, id, Transform.Default, relationships, resources);
    }
}
