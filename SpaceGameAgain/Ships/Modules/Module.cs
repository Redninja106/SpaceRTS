using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Modules;
internal abstract class Module
{
    public Ship Ship { get; }

    public Module(Ship ship)
    {
        this.Ship = ship;
    }

    public abstract Element[] BuildGUI();
    public abstract void Update();
    public abstract void Render(ICanvas canvas);
    public abstract void RenderSelected(ICanvas canvas);
}
