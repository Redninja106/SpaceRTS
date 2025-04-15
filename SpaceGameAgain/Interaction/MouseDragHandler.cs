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
        if (World.middleMouse.Holding)
        {
            DoubleVector mousePos = World.Camera.Transform.Position + DoubleVector.FromVector2(World.Camera.ScreenToLocal(Program.ViewportMousePosition));
            World.Camera.Transform.Position += World.middleMouse.DragStart - mousePos;
        }
    }
}
