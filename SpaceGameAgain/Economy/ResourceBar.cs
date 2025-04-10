using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Economy;
internal class ResourceBar : GUIWindow
{
    public ResourceBar()
    {
        Visible = true;
        this.Anchor = Alignment.BottomLeft;
    }

    public override void Layout()
    {
        foreach (var (proto, values) in World.PlayerTeam.Actor!.resources)
        {
            Text(proto.Name + ": " + values.Remaining);
        }

        base.Layout();
    }

    public override void Update(GUIViewport viewport)
    {
        base.Update(viewport);
    }
}
