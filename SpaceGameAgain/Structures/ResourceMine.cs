using SpaceGame.Economy;
using SpaceGame.Teams;
using SpaceGame.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;

[Serializable]
internal class ResourceMine : Structure
{
    public override ResourceMinePrototype Prototype => (ResourceMinePrototype)base.Prototype;

    [Serialize]
    private ResourcePrototype? resource;

    public ResourceMine(StructurePrototype prototype, ulong id) : base(prototype, id)
    {
        // resource = Grid.GetCell(location)!.Tile?.Resource;
        
        // if (resource != null)
        // {
        //     team.GetResource(resource).Capacity += 1;
        // }
    }

    public override void FinishDeserialization()
    {
        base.FinishDeserialization();
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void OnDestroyed()
    {
        if (resource != null)
        {
            Team.GetResource(resource).Capacity -= 1;
        }
        base.OnDestroyed();
    }
}

class ResourceMinePrototype : StructurePrototype
{
    public override Type ActorType => typeof(ResourceMine);

    //public override Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    //{
    //    return new ResourceMine(this, id, grid, location, rotation, team);
    //}
}


