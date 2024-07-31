using SpaceGame.GUI;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Modules;
internal class ConstructionModule(Ship ship) : Module(ship)
{
    public override Element[] BuildGUI()
    {
        return [
            new Label("buildings", 16, Alignment.Center),
            BuildButton(StructureList.ChaingunTurret),
            BuildButton(StructureList.MissileTurret),
            BuildButton(StructureList.Generator),
            BuildButton(StructureList.Manufactory),
            BuildButton(StructureList.Headquarters),
            BuildButton(StructureList.Shipyard),
            BuildButton(StructureList.ParticleAccelerator),
        ];

        TextButton BuildButton(Structure structure)
        {
            return new TextButton($"{structure.Name} | {structure.Price}m", () => 
            {
                if (Ship.Team.Materials >= structure.Price)
                {
                    World.ConstructionInteractionContext.BeginPlacing(structure);
                }
            })
            {
                FitContainer = true,
                Alignment = Alignment.Center,
            };
        }
    }

    public override void Update()
    {
    }

    public override void Render(ICanvas canvas)
    {
    }

    public override void RenderSelected(ICanvas canvas)
    {
    }
}
