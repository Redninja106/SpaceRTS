using SpaceGame.GUI;
using SpaceGame.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Modules;

[Serializable]
internal abstract class Module(ModulePrototype prototype, GameWorld world, ulong id) : Actor(prototype, world, id), IGUIProvider
{
    public override ModulePrototype Prototype => (ModulePrototype)base.Prototype;

    [field: Serialize]
    public required Ship Ship { get; set; }

    public abstract void RenderSelected(ICanvas canvas);

    public virtual void Layout(GUIWindow window)
    {
        window.Text(Prototype.Title);
        window.Separator();
    }

    public virtual void Activate()
    {
    }
}

abstract class ModulePrototype : Prototype
{
    public string Title { get; set; } = "Unknown Module";
    public Icon Icon { get; set; }

    public override Module CreateActor(GameWorld world, ulong id) => (Module)base.CreateActor(world, id);

    public override void InitializePrototype()
    {
        base.InitializePrototype();

        Icon ??= Icon.Default;
    }
}