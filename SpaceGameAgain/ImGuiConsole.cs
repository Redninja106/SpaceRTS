using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal static class ImGuiConsole
{
    private static bool open;
    private static string buf = "";

    public static void SetIntercepts()
    {
        Console.SetOut(new ImGuiConsoleWriter());
    }

    public static void Layout()
    {
        if (Keyboard.IsKeyReleased(Key.F1))
        {
            open = !open;
        }

        if (open && ImGui.Begin("Console", ref open))
        {
            ImGui.TextWrapped(buf);
        }
        ImGui.End();

    }

    private class ImGuiConsoleWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.Default;

        public override void Write(char value)
        {
            buf += value;
        }
    }
}
