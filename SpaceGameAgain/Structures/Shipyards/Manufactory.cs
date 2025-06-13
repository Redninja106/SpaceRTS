using SpaceGame.Planets;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures.Shipyards;

[Serializable]
internal class Manufactory : Structure
{
    public Manufactory(StructurePrototype prototype, ulong id) : base(prototype, id)
    {
    }
}

class ManufactoryPrototype : StructurePrototype
{
    public override Type ActorType => typeof(Manufactory);

    //public override Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    //{
    //    return new Manufactory(this, id, grid, location, rotation, team);
    //}
}