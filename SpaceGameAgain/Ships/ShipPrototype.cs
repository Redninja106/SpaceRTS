using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships;
internal class ShipPrototype : UnitPrototype
{
    public override WorldActor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        Transform transform = reader.ReadTransform();
        ActorReference<Team> team = reader.ReadActorReference<Team>();
        float height = reader.ReadSingle();

        var ship = new Ship(this, id, transform, team, height);
        int moduleCount = reader.ReadInt32();
        for (int i = 0; i < moduleCount; i++)
        {
            ship.modules.Add(reader.ReadActorReference<Module>());
        }

        return ship;
    }
}
