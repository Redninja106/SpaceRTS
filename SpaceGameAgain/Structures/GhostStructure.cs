using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class GhostStructure : Structure
{
    public override StructurePrototype Prototype => base.Prototype;

    public StructurePrototype GhostPrototype;

    public GhostStructure(StructurePrototype prototype, ulong id, ActorReference<Grid> grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid, location, rotation, team)
    {
    }

    public override void Render(ICanvas canvas)
    {
        GhostPrototype.Model.Render(canvas, this.Transform, new(1, 1, 1, .25f));
        base.Render(canvas);
    }
}
