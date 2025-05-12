using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class UrbanDistrictPrototype : StructurePrototype
{
    public int PayoutInterval { get; set; }
    public int PayoutAmount { get; set; }

    public override Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    {
        return new UrbanDistrict(this, id, grid, location, rotation, team);
    }
}
