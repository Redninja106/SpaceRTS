using SpaceGame.Orders;
using SpaceGame.Ships;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;

[Serializable]
internal class ConstructionCommand : Command
{
    [Serialize] public required Ship ship;
    [Serialize] public required Grid Grid;
    [Serialize] public required HexCoordinate Location;
    [Serialize] public required int Rotation;
    [Serialize] public required StructurePrototype Structure;

    //public ConstructionCommand(Ship ship, Grid grid, HexCoordinate location, int rotation, StructurePrototype structure)
    //{
    //    this.ship = ship;
    //    Grid = grid;
    //    Location = location;
    //    Rotation = rotation;
    //    Structure = structure;
    //}

    public override void Apply()
    {
        ShipNavigator navigator = new(ship.World);
        DoubleVector target = DoubleVector.FromVector2(Grid.Transform.LocalToWorld(Location.ToCartesian()) + Structure.Center.Rotated(Rotation * MathF.Tau / 6f));

        List<MoveOrder> path = navigator.GetPath(ship, ship.World.GetPlanetRelativePosition(target));

        var order = new ConstructionOrder()
        {
            Grid = Grid,
            Structure = Structure,
            Location = Location, 
            Rotation = Rotation
        };

        foreach (MoveOrder moveOrder in path)
        {
            ship.EnqueueOrder(moveOrder);
        }
        ship.EnqueueOrder(order);
    }

    //public override void Serialize(BinaryWriter writer)
    //{
    //    writer.Write(ship.AsReference());
    //    writer.Write(Grid);

    //    writer.Write(Location);
    //    writer.Write(Rotation);
    //    writer.Write(Structure.Name);
    //}
}

//class ConstructionCommandPrototype : CommandPrototype
//{
//    public override ConstructionCommand Deserialize(BinaryReader reader)
//    {
//        var ship = reader.ReadActorReference<Ship>();
//        var grid = reader.ReadActorReference<Grid>();

//        var location = reader.ReadHexCoordinate();
//        var rotation = reader.ReadInt32();
//        var structure = Prototypes.Get<StructurePrototype>(reader.ReadString());

//        return new ConstructionCommand(this, ship, grid, location, rotation, structure);
//    }

//}
