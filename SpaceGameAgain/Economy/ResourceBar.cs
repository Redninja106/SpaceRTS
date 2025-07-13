using SpaceGame.GUI;
using SpaceGame.Rendering;
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
        window.Text("$" + Program.World.PlayerTeam.Money.ToString() + "k");

        //foreach (var (proto, values) in Program.World.PlayerTeam.resources)
        //{
        //    window.Text(proto.Name + ": " + values.Remaining);
        //}

        //using (window.Row())
        //{
        //    window.Image(Icon.Get("economic_icon").Texture16x16);
        //    window.Text("42");
        //    window.Image(Icon.Get("industrial_icon").Texture16x16);
        //    window.Text("69");
        //    window.Image(Icon.Get("research_icon").Texture16x16);
        //    window.Text("1 million");
        //}
    }
}
