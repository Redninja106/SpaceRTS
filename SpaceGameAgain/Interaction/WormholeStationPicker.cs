using SpaceGame.Stations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Program;

namespace SpaceGame.Interaction;
internal class WormholeStationPicker(WormholeStation SourceStation) : IInteractionContext
{
    public void RenderBackgroundOverlay(ICanvas canvas, MouseState leftMouse, MouseState rightMouse)
    {
    }

    public void RenderGroundOverlay(ICanvas canvas, MouseState leftMouse, MouseState rightMouse)
    {
        foreach (var station in World.Stations)
        {
            if (station is WormholeStation wh)
            {
                canvas.PushState();
                wh.RenderGroundOverlay(canvas, World.Camera, true);
                canvas.PopState();
            }
        }
    }

    public void RenderSkyOverlay(ICanvas canvas, MouseState leftMouse, MouseState rightMouse)
    {
        canvas.PushState();
        canvas.Fill(new Color(255, 255, 255, 100));
        DoubleVector position = SourceStation.InterpolatedTransform.Position;
        Transform.Create(position, 0).ApplyTo(canvas, World.Camera);
        canvas.DrawLine(Vector2.Zero, (World.MousePosition - position).ToVector2());
        canvas.PopState();
    }

    public void Update(MouseState leftMouse, MouseState rightMouse)
    {
        if (rightMouse.Pressed)
        {
            World.CurrentInteractionContext = World.SelectInteractionContext;
        }
        if (leftMouse.Pressed)
        {
            foreach (var unit in World.Collision.TestPoint(World.MousePosition))
            {
                if (unit is WormholeStation otherStation && otherStation.Link == null)
                {
                    SourceStation.BeginEstablishingLink(otherStation);
                    otherStation.BeginEstablishingLink(SourceStation);

                    World.CurrentInteractionContext = World.SelectInteractionContext;
                }
            }
        }
    }
}
