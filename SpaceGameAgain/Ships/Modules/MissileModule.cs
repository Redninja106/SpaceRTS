using SpaceGame.Combat;
using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Modules;
internal class MissileModule : Module
{
    MissileSystem system;

    public MissileModule(Ship ship) : base(ship)
    {
        system = new(null, World.NewID(), Transform.Default, ship);
    }

    public override Element[] BuildGUI()
    {
        return [new DynamicLabel(() => $"missiles: {system.MissilesRemaining}/{system.SalvoSize}", Element.TextSize, Alignment.CenterLeft)];
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
