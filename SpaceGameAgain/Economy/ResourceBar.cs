using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Economy;
internal class ResourceBar
{
    public static void Layout(GUIWindow window)
    {
        window.Visible = true;
        window.Anchor = Alignment.BottomLeft;
        window.Alignment = Alignment.BottomLeft;
        window.Text("$" + World.PlayerTeam.Actor!.Money.ToString() + "k");

        foreach (var (proto, values) in World.PlayerTeam.Actor!.resources)
        {
            window.Text(proto.Name + ": " + values.Remaining);
        }
    }
}
