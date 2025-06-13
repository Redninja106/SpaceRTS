using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class UrbanDistrictPrototype : StructurePrototype
{
    public override Type ActorType => typeof(UrbanDistrict);

    public int PayoutInterval { get; set; }
    public int PayoutAmount { get; set; }

    //public override UrbanDistrict CreateStructure(ulong id, Team team, Grid grid, HexCoordinate location, int rotation)
    //{
    //    return new UrbanDistrict(this, id)
    //    {
    //        Team = team,
    //        Grid = grid,
    //        Location = location,
    //        Rotation = rotation,
    //    };
    //}
}
