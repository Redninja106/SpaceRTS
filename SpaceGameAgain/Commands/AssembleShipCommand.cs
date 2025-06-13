using SpaceGame.Structures.Shipyards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;

[Serializable]
internal class AssembleShipCommand : Command
{
    [Serialize]
    public required AssemblyBay assemblyBay;

    public override void Apply()
    {
        assemblyBay.BuildShip();
    }
}
