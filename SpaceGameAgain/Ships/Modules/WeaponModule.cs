using SpaceGame.Combat;
using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Modules;
internal class WeaponModule : Module
{
    public ActorReference<WeaponSystem> system;

    public override ITexture Icon => Icons.Defensive;

    public WeaponModule(WeaponModulePrototype prototype, ulong id, ActorReference<Ship> ship, ActorReference<WeaponSystem> system) : base(prototype, id, ship)
    {
        this.system = system;
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

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);
        writer.Write(Ship);
    }
}

class WeaponModulePrototype : ModulePrototype
{
    public WeaponSystemPrototype WeaponSystemPrototype { get; set; }

    public override WorldActor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        ActorReference<Ship> ship = reader.ReadActorReference<Ship>();
        ActorReference<WeaponSystem> weapon = reader.ReadActorReference<WeaponSystem>();

        return new WeaponModule(this, id, ship, weapon);
    }

    public override Module CreateModule(ulong id, ActorReference<Ship> ship)
    {
        WeaponSystem weaponSystem = WeaponSystemPrototype.CreateWeapon(World.NewID(), ship.Cast<Unit>());
        World.Add(weaponSystem);
        return new WeaponModule(this, id, ship, weaponSystem.AsReference());
    }
}
