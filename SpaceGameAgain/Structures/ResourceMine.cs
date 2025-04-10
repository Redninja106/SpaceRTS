using SpaceGame.Economy;
using SpaceGame.Teams;
using SpaceGame.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class ResourceMine : Structure
{
    public override ResourceMinePrototype Prototype => (ResourceMinePrototype)base.Prototype;

    private ResourcePrototype? resource;

    public ResourceMine(StructurePrototype prototype, ulong id, ActorReference<Grid> grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid, location, rotation, team)
    {
        resource = Grid.GetCell(location)!.Tile?.Prototype?.Resource;
        
        if (resource != null)
        {
            team.Actor!.GetResource(resource).Capacity += 1;
        }
    }

    public override void FinalizeDeserialization()
    {
        base.FinalizeDeserialization();
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void OnDestroyed()
    {
        if (resource != null)
        {
            Team.Actor!.GetResource(resource).Capacity -= 1;
        }
        base.OnDestroyed();
    }
}

class ResourceMinePrototype : StructurePrototype
{
    public override Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    {
        return new ResourceMine(this, id, grid, location, rotation, team);
    }
}


