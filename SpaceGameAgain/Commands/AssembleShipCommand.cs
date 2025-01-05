using SpaceGame.Structures.Shipyards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
internal class AssembleShipCommand : Command
{
    private AssemblyBay assemblyBay;

    public AssembleShipCommand(CommandPrototype prototype, AssemblyBay assemblyBay) : base(prototype)
    {
        this.assemblyBay = assemblyBay;
    }

    public override void Apply()
    {
        assemblyBay.BuildShip();
    }

    public override void Serialize(BinaryWriter writer)
    {
        // writer.Write(ID);
        writer.Write(assemblyBay.AsReference());
    }
}

class AssembleShipCommandPrototype : CommandPrototype
{
    public override Actor Deserialize(BinaryReader reader)
    {
        ActorReference<AssemblyBay> assemblyBay = reader.ReadActorReference<AssemblyBay>();

        return new AssembleShipCommand(this, assemblyBay.Actor!);
    }
}