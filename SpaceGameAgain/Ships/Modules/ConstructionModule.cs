using SpaceGame.GUI;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Modules;
internal class ConstructionModule(ConstructionModulePrototype prototype, ulong id, ActorReference<Ship> ship) : Module(prototype, id, ship)
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
            new ElementStack(
                Prototypes.RegisteredPrototypes.OfType<StructurePrototype>().Select(proto => {
                    return new TextButton(proto.Title, () => {
                        if (Ship.Actor!.Team.Actor!.Credits >= proto.Price)
                        {
                            World.ConstructionInteractionContext.BeginPlacing(proto);
                        }
                    }){FitContainer = true };
                }).ToArray()
            ),
        ];

        ImageButton BuildButton(ITexture texture, StructurePrototype structure)
        {
            return new ImageButton(texture, 16, 16, () =>
            {
                if (Ship.Actor!.Team.Actor!.Credits >= structure.Price)
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

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);
        writer.Write(Ship);
    }
}

class ConstructionModulePrototype : ModulePrototype
{

    public override Actor? Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        ActorReference<Ship> ship = reader.ReadActorReference<Ship>();

        return new ConstructionModule(this, id, ship);
    }

    public override Module CreateModule(ulong id, ActorReference<Ship> ship)
    {
        return new ConstructionModule(this, id, ship);
    }
}