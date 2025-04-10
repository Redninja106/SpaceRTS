using SpaceGame.GUI;
using SpaceGame.Ships;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Interaction;
internal class ConstructionMenu : GUIWindow
{
    Ship? constructionShip;
    private bool justShown;

    public ConstructionMenu()
    {

    }

    public void Open(Ship constructionShip, Vector2 offset)
    {
        this.constructionShip = constructionShip;
        Visible = true;
        Anchor = Alignment.BottomCenter;
        this.Offset = offset;
        justShown = true;
    }

    public override void Layout()
    {
        foreach (var proto in Prototypes.GetAll<StructurePrototype>())
        {
            Text(proto.Title);
            if (LastItemClicked(MouseButton.Left))
            {
                World.ConstructionInteractionContext.BeginPlacing(proto, constructionShip!);
                constructionShip = null;
                Visible = false;
            }
            if (LastItemHovered())
            {
                World.tooltipWindow.Show();
                World.tooltipWindow.Text(proto.Title);
            }
        }

        base.Layout();
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
}
