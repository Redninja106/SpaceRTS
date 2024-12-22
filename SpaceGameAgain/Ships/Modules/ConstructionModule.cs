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
    static ITexture defensive = Graphics.LoadTexture("./Assets/Icons/defensive.png");
    static ITexture industrial = Graphics.LoadTexture("./Assets/Icons/industrial.png");
    static ITexture economic = Graphics.LoadTexture("./Assets/Icons/economic.png");
    static ITexture research = Graphics.LoadTexture("./Assets/Icons/research.png");

    static ConstructionModule()
    {
        defensive.Filter = TextureFilter.Point;
        industrial.Filter = TextureFilter.Point;
        economic.Filter = TextureFilter.Point;
        research.Filter = TextureFilter.Point;
    }

    public override Element[] BuildGUI()
    {
        return [
            new ElementRow([
                BuildButton(research, Prototypes.Get<StructurePrototype>("particle_accelerator")),
                BuildButton(industrial, Prototypes.Get<StructurePrototype>("shipyard")),
                //BuildButton(economic, Prototypes.Get<StructurePrototype>("generator")),
                //BuildButton(defensive, Prototypes.Get<StructurePrototype>("turret")),

                //BuildButton(defensive, World.Structures.DefensiveZone),
                //BuildButton(industrial, World.Structures.IndustrialZone),
                //BuildButton(economic, World.Structures.EconomicZone),
                //BuildButton(research, World.Structures.ResearchZone),
            ]),
        ];

        ImageButton BuildButton(ITexture texture, StructurePrototype structure)
        {
            return new ImageButton(texture, 16, 16, () =>
            {
                if (Ship.Team.Actor!.Credits >= structure.Price)
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
