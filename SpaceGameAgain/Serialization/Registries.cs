using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Serialization;
internal static class Registries
{
    public static Registry<Structure> Structures { get; } = new([
        StructureList.MissileTurret,
        StructureList.ChaingunTurret,
        StructureList.Generator,
        StructureList.Headquarters,
        StructureList.Manufactory,
        StructureList.ParticleAccelerator,
        StructureList.Shipyard,
    ]);
}
