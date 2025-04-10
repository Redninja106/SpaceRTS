using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Interaction;
internal class InfoMenu : GUIWindow
{
    private Unit? unit;
    private bool justShown;

    public void Show(Unit u, Vector2 offset)
    {
        this.unit = u;

        Visible = true;
        Anchor = Alignment.BottomCenter;
        this.Offset = offset;
        justShown = true;
    }

    public override void Update(GUIViewport viewport)
    {
        base.Update(viewport);
        if (!Hovered && !justShown)
        {
            Visible = false;
        }
        justShown = false;
    }

    public override void Layout()
    {
        Text(unit!.Prototype.Title, 32);

        base.Layout();

    }
}
