using SpaceGame.Planets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Interaction;
internal class MouseDragHandler
{
    public void Update()
    {
        if (World.rightMouse.Holding)
        {
            Vector2 mousePos = World.Camera.ScreenToWorld(Program.ViewportMousePosition, false);
            World.Camera.Transform.Position += World.rightMouse.DragStart - mousePos;
        }
    }
}
