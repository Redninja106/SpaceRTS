using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Interaction;
interface IInteractionContext
{
    void Update(MouseState leftMouse, MouseState rightMouse);
    void RenderBackgroundOverlay(ICanvas canvas, MouseState leftMouse, MouseState rightMouse);
    void RenderGroundOverlay(ICanvas canvas, MouseState leftMouse, MouseState rightMouse);
    void RenderSkyOverlay(ICanvas canvas, MouseState leftMouse, MouseState rightMouse);
}
