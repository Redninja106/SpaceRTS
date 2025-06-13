using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;

[Serializable]
internal class GhostStructure : Structure
{
    public override StructurePrototype Prototype => base.Prototype;

    [Serialize]
    public StructurePrototype GhostPrototype;

    public GhostStructure(StructurePrototype prototype, ulong id) : base(prototype, id)
    {
    }

    public override void Render(ICanvas canvas)
    {
        GhostPrototype.Model.Render(canvas, this.Transform, new(1, 1, 1, .25f));
        base.Render(canvas);
    }
}
