 using SpaceGame.Combat;
using SpaceGame.GUI;
using SpaceGame.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Modules;
internal class WeaponModule : Module
{
    public override WeaponModulePrototype Prototype => (WeaponModulePrototype)base.Prototype;

    public required WeaponSystem system;

    public override ITexture Icon => Icons.Defensive;

    public WeaponModule(WeaponModulePrototype prototype, ulong id) : base(prototype, id)
    {
    }

    public override void InitializeActor()
    {
        base.InitializeActor();

        WeaponSystem weaponSystem = Prototype.WeaponSystemPrototype.CreateActor(World.NewID());
        weaponSystem.Unit = this.Ship;
        World.Add(weaponSystem);
    }

    public override void Layout(GUIWindow window)
    {
    }

    //public override Element[] BuildGUI()
    //{
    //    return [];
    //    // return [new DynamicLabel(() => $"missiles: {system.MissilesRemaining}/{system.SalvoSize}", Element.TextSize, Alignment.CenterLeft)];
    //}

    public override void Render(ICanvas canvas)
    {
    }

    public override void RenderSelected(ICanvas canvas)
    {
        // system.RenderSelected(canvas);
    }

    public override void Tick()
    {
        // system.Update();
    }

    //public override void Serialize(BinaryWriter writer)
    //{
    //    writer.Write(ID);
    //    writer.Write(system);
    //}
}

class WeaponModulePrototype : ModulePrototype
{
    public override Type ActorType => typeof(WeaponModule);

    public WeaponSystemPrototype WeaponSystemPrototype { get; set; }

    //public override Actor Deserialize(BinaryReader reader)
    //{
    //    ulong id = reader.ReadUInt64();
    //    ActorReference<Ship> ship = reader.ReadActorReference<Ship>();
    //    WeaponSystem> weapon = reader.ReadActorReference<WeaponSystem>();

    //    return new WeaponModule(this, id, ship, weapon);
    //}

    //public override Module CreateModule(ulong id, Ship ship)
    //{
    //    WeaponSystem weaponSystem = WeaponSystemPrototype.CreateActor(World.NewID());
    //    weaponSystem.Unit = ship;
    //    World.Add(weaponSystem);
    //    return new WeaponModule(this, id, ship, weaponSystem);
    //}
}
