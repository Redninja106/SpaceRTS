using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Modules;

[Serializable]
internal abstract class Module(ModulePrototype prototype, GameWorld world, ulong id) : Actor(prototype, world, id), IGUIProvider
{
    [field: Serialize]
    public required Ship Ship { get; set; }
    public abstract ITexture Icon { get; }

    //public abstract Element[] BuildGUI();
    public abstract void RenderSelected(ICanvas canvas);

    public abstract void Layout(GUIWindow window);
}

abstract class ModulePrototype : Prototype
{
    public override Module CreateActor(GameWorld world, ulong id) => (Module)base.CreateActor(world, id);
}