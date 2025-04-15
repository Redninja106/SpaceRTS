using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Economy;
internal class TooltipWindow : GUIWindow
{
    public TooltipWindow()
    {
        this.Visible = true;
    }

    public override void Update(GUIViewport viewport)
    {
        this.Offset = viewport.MousePosition - this.CalculatedBounds.Size;
        base.Update(viewport);
        Hovered = false;
    }

    public override void Render(ICanvas canvas, float displayWidth, float displayHeight)
    {
        this.Offset = World.GUIViewport.MousePosition;
        if (this.CalculatedBounds.Size != Vector2.Zero)
        {
            base.Render(canvas, displayWidth, displayHeight);
        }
    }
}
