using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Modules;
internal abstract class Module : Actor
{
    public ActorReference<Ship> Ship { get; }

    public Module(ModulePrototype prototype, ulong id, ActorReference<Ship> ship) : base(prototype, id, Transform.Default)
    {
        this.Ship = ship;
    }

    public abstract Element[] BuildGUI();
    public abstract void RenderSelected(ICanvas canvas);
}

abstract class ModulePrototype : Prototype
{
    public override Actor? Deserialize(BinaryReader reader)
    {
        return null;
    }

    public abstract Module CreateModule(ulong id, ActorReference<Ship> ship);
}