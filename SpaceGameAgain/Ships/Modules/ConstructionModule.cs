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
    public override ITexture Icon => Icons.Construction;

    static ConstructionModule()
    {
    }

    public override void Layout(GUIWindow window)
    {
        foreach (var proto in Prototypes.GetAll<StructurePrototype>())
        {
            window.Text(proto.Title);
            if (window.LastItemClicked(MouseButton.Left))
            {
                World.ConstructionInteractionContext.BeginPlacing(proto, Ship.Actor!);
            }
        }

    }

    public override Element[] BuildGUI()
    {
        return [];
        //return [
        //    new ElementStack(
        //        Prototypes.RegisteredPrototypes.OfType<StructurePrototype>().Select(proto => {
        //            return new TextButton($"{proto.Title} ({proto.Price})", () => {
        //                if (Ship.Actor!.Team.Actor!.GetResource("metals") >= proto.Price)
        //                {
        //                    World.ConstructionInteractionContext.BeginPlacing(proto, this.Ship.Actor!);
        //                }
        //            }) { FitContainer = true , Margin   = 0};
        //        }).ToArray()
        //    ),
        //];

        //ImageButton BuildButton(ITexture texture, StructurePrototype structure)
        //{
        //    return new ImageButton(texture, 16, 16, () =>
        //    {
        //        if (Ship.Actor!.Team.Actor!.Resources["metals"] >= structure.Price)
        //        {
        //            World.ConstructionInteractionContext.BeginPlacing(structure, this.Ship.Actor!);
        //        }
        //    })
        //    {
        //        FitContainer = true,
        //        Alignment = Alignment.Center,
        //    };
        //}
    }

    public override void Tick()
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

    public override WorldActor Deserialize(BinaryReader reader)
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