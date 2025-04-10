using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Economy;
internal class TooltipWindow : GUIWindow
{
    public override void Update(GUIViewport viewport)
    {
        this.Offset = viewport.MousePosition;
        base.Update(viewport);
    }

    public void Show()
    {
        this.Visible = true;
    }
}
