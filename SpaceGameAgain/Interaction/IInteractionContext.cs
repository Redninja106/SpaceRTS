using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Interaction;
internal interface IInteractionContext
{
    void Update(MouseState leftMouse, MouseState rightMouse);
    void Render(ICanvas canvas, MouseState leftMouse, MouseState rightMouse);
}
