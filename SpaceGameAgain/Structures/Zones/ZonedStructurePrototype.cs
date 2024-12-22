using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class ZonedStructurePrototype : StructurePrototype
{
    public ColorF Color { get; }

    public ZonedStructurePrototype(string name, int price, Model model, HexCoordinate[] footprint, ColorF color) : base(name, price, model, null, footprint)
    {
        this.Color = color;
    }
}
