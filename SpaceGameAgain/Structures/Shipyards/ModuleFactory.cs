using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures.Shipyards;

[Serializable]
internal class ModuleFactory(StructurePrototype prototype, GameWorld world, ulong id) : Structure(prototype, world, id)
{
    public override ModuleFactoryPrototype Prototype => (ModuleFactoryPrototype)base.Prototype;
}

class ModuleFactoryPrototype : StructurePrototype
{
    public override Type ActorType => typeof(ModuleFactory);

    public ModulePrototype ProvidedModule { get; set; }

    //public override Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    //{
        //return new ModuleFactory(this, id, grid, location, rotation, team);
    //}
}
