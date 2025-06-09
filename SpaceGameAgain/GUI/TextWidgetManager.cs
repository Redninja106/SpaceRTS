using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
internal class TextWidgetManager
{
    // EVENT WIDGETS are produced without user interaction by units like the
    // urban district. They don't scale up to be always legible and are hidden
    // by fog of war.
    private readonly List<TextWidget> eventWidgets = [];


    // NOTIFICATION WIDGETS are produced to notify the player of something, like
    // if they try to build a structure where they can't. These scale to be always
    // legible and aren't effect by fog of war
    private readonly List<TextWidget> notificationWidgets = [];

    public TextWidgetManager()
    {
    }

    public void Tick()
    {
        TickWidgetList(eventWidgets);
        TickWidgetList(notificationWidgets);
    }

    private static void TickWidgetList(List<TextWidget> widgets)
    {
        for (int i = 0; i < widgets.Count; i++)
        {
            var widget = widgets[i];
            widget.Tick();

            if (widget.Age > Program.TickRate * 2)
            {
                widgets.RemoveAt(i);
                i--;
            }
        }
    }

    public void RenderEventWidgets(ICanvas canvas, Camera camera)
    {
        foreach (var widget in eventWidgets)
        {
            widget.Render(canvas, camera, false);
        }
    }

    public void RenderNotificationWidgets(ICanvas canvas, Camera camera)
    {
        foreach (var widget in notificationWidgets)
        {
            widget.Render(canvas, camera, true);
        }
    }

    public void AddEventWidget(TextWidget widget)
    {
        this.eventWidgets.Add(widget);
    }

    public void AddNotficationWidget(TextWidget widget)
    {
        this.notificationWidgets.Add(widget);
    }
}
