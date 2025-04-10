using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Modules;
internal abstract class Module : WorldActor, IGUIProvider
{
    public ActorReference<Ship> Ship { get; }
    public abstract ITexture Icon { get; }

    public Module(ModulePrototype prototype, ulong id, ActorReference<Ship> ship) : base(prototype, id, Transform.Default)
    {
        this.Ship = ship;
    }

    public abstract Element[] BuildGUI();
    public abstract void RenderSelected(ICanvas canvas);

    public abstract void Layout(GUIWindow window);
}

abstract class ModulePrototype : WorldActorPrototype
{
    public override WorldActor Deserialize(BinaryReader reader)
    {
        return null;
    }

    public abstract Module CreateModule(ulong id, ActorReference<Ship> ship);
}