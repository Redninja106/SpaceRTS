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

    public ResourceDepositTile? tile;

    public ResourceMine(StructurePrototype prototype, ulong id, ActorReference<Grid> grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid, location, rotation, team)
    {
        tile = Grid.GetCell(location)!.Tile as ResourceDepositTile;
    }

    public override void Tick()
    {
        base.Tick();

        if (tile != null && World.tick % (ulong)Prototype.MiningFrequency == 0)
        {
            tile.ResourceCount -= Prototype.MiningCount;
            Team.Actor!.Resources[tile.Prototype.Resource] += Prototype.MiningCount;
        }
    }
}

class ResourceMinePrototype : StructurePrototype
{
    public int MiningFrequency { get; set; }
    public int MiningCount { get; set; }


    public override Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    {
        return new ResourceMine(this, id, grid, location, rotation, team);
    }
}


