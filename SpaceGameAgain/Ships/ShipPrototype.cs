using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships;
internal class ShipPrototype : UnitPrototype
{
    public override Type ActorType => typeof(Ship);

    public override Actor? Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        Transform transform = reader.ReadTransform();
        ActorReference<Team> team = reader.ReadActorReference<Team>();
        float height = reader.ReadSingle();

        return new Ship(this, id, transform, team, height);
    }
}
