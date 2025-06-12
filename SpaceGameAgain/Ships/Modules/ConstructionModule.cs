using SpaceGame.Extensions;
using SpaceGame.GUI;
using SpaceGame.Rendering;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Modules;
internal class ConstructionModule(ConstructionModulePrototype prototype, ulong id, ActorReference<Ship> ship) : Module(prototype, id, ship)
{
    public override ConstructionModulePrototype Prototype => (ConstructionModulePrototype)base.Prototype;
    public override ITexture Icon => Icons.Construction;

    static ConstructionModule()
    {
    }

    public override void Layout(GUIWindow window)
    {
        foreach (var group in Prototype.BuildableStructuresByCategory)
        {
            window.Text(group.Key, size: 20);
            window.Separator();
            foreach (var proto in group)
            {
                bool canAfford = World.PlayerTeam.Actor!.Money >= proto.Cost;
                window.Text(proto.Title, size: 16, color: canAfford ? Color.Gray : Color.Red);
                if (window.LastItemHovered())
                {
                    World.GUIViewport.SetTooltip(proto.Layout);
                }
                if (window.LastItemClicked(MouseButton.Left) && canAfford)
                {
                    World.ConstructionInteractionContext.BeginPlacing(proto, Ship.Actor!);
                    World.GUIViewport.ClosePopup();
                }
            }
        }

        //foreach (var proto in Prototype.BuildableStructures)
        //{
        //    bool canAfford = World.PlayerTeam.Actor!.Money >= proto.Cost;

        //    window.Text(proto.Title, color: canAfford ? null : Color.Red);
        //    if (window.LastItemClicked(MouseButton.Left) && canAfford)
        //    {
        //        World.ConstructionInteractionContext.BeginPlacing(proto, Ship.Actor!);
        //    }
        //    if (window.LastItemHovered())
        //    {
        //        World.SetTooltip(w => proto.Layout(w));
        //    }
        //}

    }

    //public override Element[] BuildGUI()
    //{
    //    return [];
    //    //return [
    //    //    new ElementStack(
    //    //        Prototypes.RegisteredPrototypes.OfType<StructurePrototype>().Select(proto => {
    //    //            return new TextButton($"{proto.Title} ({proto.Price})", () => {
    //    //                if (Ship.Actor!.Team.Actor!.GetResource("metals") >= proto.Price)
    //    //                {
    //    //                    World.ConstructionInteractionContext.BeginPlacing(proto, this.Ship.Actor!);
    //    //                }
    //    //            }) { FitContainer = true , Margin   = 0};
    //    //        }).ToArray()
    //    //    ),
    //    //];

    //    //ImageButton BuildButton(ITexture texture, StructurePrototype structure)
    //    //{
    //    //    return new ImageButton(texture, 16, 16, () =>
    //    //    {
    //    //        if (Ship.Actor!.Team.Actor!.Resources["metals"] >= structure.Price)
    //    //        {
    //    //            World.ConstructionInteractionContext.BeginPlacing(structure, this.Ship.Actor!);
    //    //        }
    //    //    })
    //    //    {
    //    //        FitContainer = true,
    //    //        Alignment = Alignment.Center,
    //    //    };
    //    //}
    //}

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
    public StructurePrototype[] BuildableStructures { get; set; } = [];
    public IGrouping<string, StructurePrototype>[] BuildableStructuresByCategory { get; set; } = [];

    public override void InitializePrototype()
    {
        BuildableStructuresByCategory = BuildableStructures.OrderBy(s => s.Title).GroupBy(s => s.Category).ToArray();
        base.InitializePrototype();
    }

    public override Actor Deserialize(BinaryReader reader)
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