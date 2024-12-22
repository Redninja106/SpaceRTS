using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures.Zones;
internal class ZoneStructure : Structure
{
    public ZoneStructure(ZoneStructurePrototype prototype, ulong id, Grid grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid, location, rotation, team)
    {
    }
}

class ZoneStructurePrototype : StructurePrototype
{
    public ZoneStructurePrototype(string title, int price, Model model, string? presetModel) : base(title, price, model, presetModel, [new(0, 0)])
    {

    }
}
