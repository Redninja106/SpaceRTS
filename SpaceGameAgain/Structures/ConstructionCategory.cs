using SpaceGame.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class ConstructionCategory : DataPrototype
{
    public required string Title { get; set; }
    public required Icon Icon { get; set; }

    public override void InitializePrototype()
    {
        base.InitializePrototype();
        Icon ??= Icon.Default;
    }
}
