using SpaceGame.GUI;
using SpaceGame.Planets;
using SpaceGame.Ships.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class ChatMenu
{
    public List<(string text, ulong tick)> messages = [];
    public ulong lastActivityTick;
    public GameWorld world;

    public ChatMenu(GameWorld world)
    {
        this.world = world;
    }

    public void Layout(GUIWindow window)
    {
        window.RenderBackground = false;
        window.Anchor = Alignment.BottomLeft;
        window.Alignment = Alignment.BottomLeft;
        window.Offset.Y = -100;

        float alpha = ((long)world.tick - (long)lastActivityTick - (long)Program.UserOptions.ChatFadeDelay) / 50f;
        alpha = 1f - float.Clamp(alpha, 0, 1);

        window.Visible = alpha > 0;

        window.MinSize(new Vector2(100, 100));

        for (int i = 0; i < messages.Count; i++)
        {
            var (text, tick) = messages[i];
            ColorF color = GUIWindow.DefaultTextColor.ToColorF();
            color.A = GetElementAlpha(tick);
            window.Text(text, color: color.ToColor());
            if (color.A == 0)
            {
                messages.RemoveAt(i);
                i--;
            }
        }
    }

    private float GetElementAlpha(ulong tick)
    {
        float a = ((long)world.tick - (long)tick - (long)Program.UserOptions.ChatFadeDelay) / Program.TickRate;
        return 1f - float.Clamp(a, 0, 1);
    }

    public void AddMessage(string message)
    {
        this.messages.Add((message, world.tick));
        lastActivityTick = world.tick;
    }
}
