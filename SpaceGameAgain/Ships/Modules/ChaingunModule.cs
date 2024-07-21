using SpaceGame.Combat;
using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Modules;
internal class ChaingunModule : Module
{
    ChaingunSystem system;

    public ChaingunModule(Ship ship) : base(ship)
    {
        system = new(ship);
    }

    public override Element[] BuildGUI()
    {
        return [new DynamicLabel(() => $"rounds: {system.ammo}/{system.ammoCapacity}", Element.TextSize)];
    }

    public override void Render(ICanvas canvas)
    {
    }

    public override void RenderSelected(ICanvas canvas)
    {
        system.RenderSelected(canvas);
    }

    public override void Update()
    {
        system.Update();
    }
}
