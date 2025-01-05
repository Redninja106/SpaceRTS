using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Orders;
internal class ConstructionOrderPrototype : OrderPrototype
{
    public override WorldActor Deserialize(BinaryReader reader)
    {
        DeserializeArgs(reader, out var id, unit: out var unit);
        ActorReference<Grid> grid = reader.ReadActorReference<Grid>();
        StructurePrototype structurePrototype = Prototypes.Get<StructurePrototype>(reader.ReadString());
        HexCoordinate location = reader.ReadHexCoordinate();
        int rotation = reader.ReadInt32();

        return new ConstructionOrder(this, id, unit, grid, structurePrototype, location, rotation);
    }
}
