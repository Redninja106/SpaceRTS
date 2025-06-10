using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Economy;
//internal class TooltipManager
//{
//    GUIWindow window;
//    private Action<GUIWindow>? onLayoutTooltip = null;

//    public TooltipManager()
//    {
//        window.Visible = true;
//    }

//    public void Update(GUIViewport viewport)
//    {
//        window.Offset = viewport.MousePosition - window.CalculatedBounds.Size;
//        // base.Update(viewport);
//        window.Hovered = false;
//    }

//    public void Render(ICanvas canvas, float displayWidth, float displayHeight)
//    {
//        //window.UpdateLayout();

//        window.Offset = World.GUIViewport.MousePosition;
//        if (window.CalculatedBounds.Size != Vector2.Zero)
//        {
//            base.(canvas, displayWidth, displayHeight);
//        }
//    }

//    public void Set(Action<GUIWindow> onLayoutTooltip)
//    {
//        this.onLayoutTooltip = onLayoutTooltip;
//    }
//}
