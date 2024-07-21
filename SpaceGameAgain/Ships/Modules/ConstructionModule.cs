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
            BuildButton(World.Structures.ChaingunTurret),
            BuildButton(World.Structures.MissileTurret),
            BuildButton(World.Structures.Generator),
            BuildButton(World.Structures.Manufactory),
            BuildButton(World.Structures.Headquarters),
            BuildButton(World.Structures.Shipyard),
            BuildButton(World.Structures.ParticleAccelerator),
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
