using ImGuiNET;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal static class DebugMenu
{
    public static bool Open = false;
    private static bool ObjectViewerOpen = false;
    private static bool focusObjectViewer;
    private static object? objectViewerObject;

    public static void Layout()
    {
        if (Keyboard.IsKeyPressed(Key.F1))
            Open = !Open;

        if (Open && ImGui.Begin("debug menu", ref Open, ImGuiWindowFlags.MenuBar))
        {
            if (ImGui.BeginMenuBar())
            {
                LayoutFileMenuBar();

                ImGui.EndMenuBar();
            }

            if (ImGui.BeginTabBar("tabbar"))
            {
                if (ImGui.BeginTabItem("world"))
                {
                    LayoutWorldTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("prototypes"))
                {
                    LayoutPrototypesTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("editor"))
                {
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("settings"))
                {
                    LayoutSettingsTab();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
        ImGui.End();

        if (focusObjectViewer)
        {
            focusObjectViewer = false;
            ImGui.SetNextWindowFocus();
        }

        if (ObjectViewerOpen && ImGui.Begin("object viewer", ref ObjectViewerOpen))
        {
            LayoutObjectViewer();
        }
        ImGui.End();
    }

    private static void LayoutSettingsTab()
    {
        ImGui.SliderFloat("game speed", ref Program.GameSpeed, 0.0f, 2);
        ImGui.SameLine();
        if (ImGui.SmallButton("reset"))
        {
            Program.GameSpeed = 1;
        }
        if (ImGui.SmallButton("tick"))
        {
            Program.forceTickThisFrame = true;
        }
    }

    private static void LayoutPrototypesTab()
    {
        foreach (var prototype in Prototypes.RegisteredPrototypes)
        {
            if (ImGui.Selectable($"{prototype.Name} ({prototype.GetType().Name})"))
            {
                ViewObject(prototype);
            }
        }
    }

    private static void LayoutObjectViewer()
    {
        if (objectViewerObject == null)
        {
            ImGui.Text("no actor selected");
            return;
        }

        if (objectViewerObject is StructurePrototype structurePrototype && ImGui.Button("place"))
        {
            World.ConstructionInteractionContext.BeginPlacing(structurePrototype);
        }

        if (objectViewerObject is IInspectable inspectable)
        {
            inspectable.Layout();
        }
        else
        {
            ImGui.Text(objectViewerObject.ToString());
        }
    }

    private static void LayoutFileMenuBar()
    {
        if (ImGui.BeginMenu("file"))
        {
            if (ImGui.MenuItem("save"))
            {
                WorldSerializer serializer = new();
                using var fs = new FileStream("./level", FileMode.Create);
                BinaryWriter writer = new(fs, Encoding.UTF8);
                serializer.Serialize(World, writer);
            }
            if (ImGui.MenuItem("load"))
            {
                WorldSerializer serializer = new();
                using var fs = new FileStream("./level", FileMode.Open);
                BinaryReader reader = new(fs, Encoding.UTF8);
                World = serializer.Deserialize(reader);
            }

            ImGui.EndMenu();
        }
    }

    private static void ViewObject(object obj)
    {
        objectViewerObject = obj;
        focusObjectViewer = true;
        ObjectViewerOpen = true;
    }

    private static void LayoutWorldTab()
    {
        foreach (var actor in World.Actors.Values)
        {
            if (ImGui.Selectable(actor.ToString()))
            {
                ViewObject(actor);
            }
        }
    }
}