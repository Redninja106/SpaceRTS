using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures.Shipyards;
internal class ModuleFactory : Structure
{
    public override ModuleFactoryPrototype Prototype => (ModuleFactoryPrototype)base.Prototype;

    public ModuleFactory(StructurePrototype prototype, ulong id, ActorReference<Grid> grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid, location, rotation, team)
    {
    }
}

class ModuleFactoryPrototype : StructurePrototype
{
    public string ProvidedModule { get; set; }

    public ModuleFactoryPrototype(string title, int price, Model model, string? presetModel, HexCoordinate[] footprint) : base(title, price, model, presetModel, footprint)
    {
    }

    public override Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    {
        return new ModuleFactory(this, id, grid, location, rotation, team);
    }
}
