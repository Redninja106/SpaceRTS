﻿using ImGuiNET;
using Silk.NET.Core.Native;
using SpaceGame.Commands;
using SpaceGame.Networking;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal static class DebugMenu
{
    public static bool Open = false;
    private static ObjectViewer objectViewer = new();

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

                if (ImGui.BeginTabItem("network"))
                {
                    LayoutNetworkTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("overlays"))
                {
                    DebugOverlays.Layout();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
        ImGui.End();

        if (objectViewer.WantsFocus)
        {
            objectViewer.WantsFocus = false;
            ImGui.SetNextWindowFocus();
        }

        if (objectViewer.Open && ImGui.Begin("object viewer", ref objectViewer.Open, ImGuiWindowFlags.MenuBar))
        {
            objectViewer.Layout();
        }
        ImGui.End();
    }


    private static string addressOrPort = "45454";
    private static void LayoutNetworkTab()
    {
        ImGui.InputText("address/port", ref addressOrPort, 64);

        if (ImGui.Button("host"))
        {
            if (ParseAddressAndPort(out var _, out var port))
            {
                NetworkHost host = new(port);
                Program.Lobby = new LocalLobby(host);
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("join"))
        {
            if (ParseAddressAndPort(out var address, out var port))
            {
                NetworkClient client = new(address, port);
                Program.Lobby = new NetworkLobby(client);
            }
        }

        static bool ParseAddressAndPort([NotNullWhen(true)] out string? address, out int port)
        {
            string[] addressParts = addressOrPort.Split(":");

            if (addressParts.Length == 2 && int.TryParse(addressParts[1], out port))
            {
                address = addressParts[0];
                return true;
            }
            
            if (int.TryParse(addressParts[0], out port))
            {
                address = "localhost";
                return true;
            }

            address = null;
            port = 0;
            return false;
        }

        ImGui.SeparatorText("Command buffers");

        foreach (var team in World.Teams)
        {
            if (ImGui.TreeNode($"team {team.ID}: {team.CommandProcessor?.GetType()?.Name ?? "(none)"}")) 
            {
                Dictionary<ulong, Command[]>? commands = team.CommandProcessor switch
                {
                    NetworkCommandProcessor network => network.commands,
                    PlayerCommandProcessor player => player.commands,
                    _ => null
                };

                if (commands != null)
                {
                    foreach (var (turn, cmds) in commands)
                    {
                        if (ImGui.TreeNode("turn " + turn))
                        {
                            foreach (var cmd in cmds)
                            {
                                ImGui.Text(cmd.ToString());
                            }

                            ImGui.TreePop();
                        }
                    }
                }
                else
                {
                    ImGui.Text("(none)");
                }

                ImGui.TreePop();
            }
        }
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

    private static DebugSearch<Prototype>? prototypeSearch = null;
    private static void LayoutPrototypesTab()
    {
        prototypeSearch ??= new DebugSearch<Prototype>(() => Prototypes.RegisteredPrototypes.ToList(), proto => [proto.Name, proto.ToString()!]);

        prototypeSearch.Layout();
        prototypeSearch.Prune();

        foreach (var prototype in prototypeSearch.QueryItems)
        {
            if (ImGui.Selectable($"{prototype.Name} ({prototype.GetType().Name})"))
            {
                objectViewer.View(prototype);
            }
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



    private static DebugSearch<WorldActor>? actorList;
    private static void LayoutWorldTab()
    {
        ImGui.Text($"turn: {World.TurnProcessor.turn}");
        ImGui.Text($"remaining ticks: {World.TurnProcessor.RemainingTicks}");
        
        float tickProgress = MathF.Min(Program.timeAccumulated * Program.GameSpeed / Program.Timestep, 1);
        if (Program.GameSpeed == 0)
        {
            tickProgress = 1;
        }
        ImGui.Text($"tick progress: {tickProgress}");

        ImGui.Separator();

        ImGui.Text("next id:" + World.NextID);
        
        ImGui.Separator();

        if (ImGui.TreeNode("camera"))
        {
            World.Camera.Layout();
            ImGui.TreePop();
        }

        ImGui.SeparatorText("actors");

        actorList ??= new(() => World.Actors.Values.ToList(), a => [a.ToString(), a.Prototype.Name]);
        actorList.Layout();
        actorList.Prune();

        foreach (var actor in actorList.QueryItems)
        {
            if (ImGui.Selectable(actor.ToString()))
            {
                objectViewer.View(actor);
            }
        }
    }
}

class DebugSearch<T>
{
    private string query = "";
    private int pruneIndex = 0;
    private Func<T, string[]>? stringProvider;
    private Func<List<T>> allItems;

    public List<T> QueryItems { get; private set; }

    public string Query 
    { 
        get => query; 
        set
        {
            query = value;
            pruneIndex = 0;
        }
    }

    public DebugSearch(Func<List<T>> allItems, Func<T, string[]>? stringProvider = null)
    {
        this.allItems = allItems;
        this.stringProvider = stringProvider;

        this.QueryItems = this.allItems();
    }

    public void Layout()
    {
        string prevQuery = query;
        if (ImGui.InputTextWithHint("##search", "search", ref query, 128))
        {
            pruneIndex = 0;
            if (!query.Contains(prevQuery))
            {
                QueryItems = allItems();
            }
        }
    }

    public void Prune(int count = 10)
    {
        for (int i = 0; i < count; i++)
        {
            if (QueryItems.Count > pruneIndex)
            {
                T item = QueryItems[pruneIndex];
                string[] strings = stringProvider?.Invoke(item) ?? [item?.ToString() ?? ""];

                if (strings.Any(s => s != null && s.Contains(query, StringComparison.InvariantCultureIgnoreCase)))
                {
                    pruneIndex++;
                }
                else
                {
                    QueryItems.RemoveAt(pruneIndex);
                }
            }
        }
    }
}

class ObjectViewer
{
    public bool Open = false;
    public bool WantsFocus = false;

    private object? focusObject = null;
    private HashSet<object> targets = [];
    public void Layout()
    {
        if (ImGui.BeginMenuBar())
        {
            ImGui.EndMenuBar();
        }

        if (ImGui.BeginTabBar("tabbar"))
        {
            foreach (var target in targets)
            {
                bool open = true;
                ImGuiTabItemFlags flags = 0;
                if (target == focusObject)
                {
                    focusObject = null;
                    flags = ImGuiTabItemFlags.SetSelected;
                }
                if (ImGui.BeginTabItem(target.ToString(), ref open, flags))
                {
                    if (target is IInspectable inspectable)
                    {
                        inspectable.Layout();
                    }
                    else
                    {
                        ReflectionLayoutObjectFields(target);
                    }
                    ImGui.EndTabItem();
                }
                if (!open)
                {
                    targets.Remove(target);

                    if (targets.Count == 0)
                    {
                        this.Open = false;
                    }
                }
            }

            ImGui.EndTabBar();
        }
    }

    public object? ReflectionLayoutObject(string label, object? obj, bool isReadonly = false)
    {
        switch (obj)
        {
            case float f when isReadonly:
                ImGui.Text($"{label}: {f}");
                return f;
            case float f:
                ImGui.DragFloat(label, ref f);
                return f;
            case Vector2 v2:
                ImGui.DragFloat2(label, ref v2);
                return v2;
            case DoubleVector d2:
                Vector2 vec = d2.ToVector2();
                ImGui.DragFloat2(label, ref vec);
                return DoubleVector.FromVector2(vec);
            case int i:
                ImGui.DragInt(label, ref i);
                return i;
            case bool b:
                ImGui.Checkbox(label, ref b);
                return b;
            case string s:
                ImGui.InputText(label, ref s, 256);
                return s;
            case Prototype proto:
                if (ImGui.Selectable($"{label}: {proto.Name} ({proto.GetType().Name})"))
                {
                    View(proto);
                }
                return proto;
            case IInspectable inspectable:
                if (ImGui.TreeNode(label))
                {
                    inspectable.Layout();
                    ImGui.TreePop();
                }
                return inspectable;
            case null:
            case object when obj.GetType().IsPrimitive:
                ImGui.Text($"{label}: {obj ?? "null"}");
                return obj;
            case object:
                if (ImGui.TreeNode(label))
                {
                    ReflectionLayoutObjectFields(obj);
                    ImGui.TreePop();
                }
                return obj;
        }
    }

    public void ReflectionLayoutObjectFields(object obj)
    {
        foreach (var member in obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance))
        {
            object? value;
            switch (member)
            {
                case FieldInfo field:
                    value = field.GetValue(obj);
                    value = ReflectionLayoutObject(field.Name, value);
                    if (!field.Attributes.HasFlag(FieldAttributes.InitOnly))
                    {
                        field.SetValue(obj, value);
                    }
                    break;
                case PropertyInfo prop:
                    if (prop.CanRead && prop.GetIndexParameters().Length == 0)
                    {
                        value = prop.GetValue(obj);
                        bool isReadonly = !prop.CanWrite;

                        if (isReadonly)
                        {
                            ImGui.BeginDisabled();
                        }

                        value = ReflectionLayoutObject(prop.Name, value, isReadonly);
                        if (isReadonly)
                        {
                            ImGui.EndDisabled();
                        }
                        else
                        {
                            prop.SetValue(obj, value);
                        }
                    }
                    break;
                case MethodInfo method when method.GetCustomAttribute<DebugButtonAttribute>() != null:
                    if (ImGui.Button(method.Name))
                    {
                        method.Invoke(obj, null);
                    }
                    break;
                default:
                    break;
            }
        }

        //foreach (var field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        //{
        //}
    }

    public void View(object obj)
    {
        if (!targets.Contains(obj))
        {
            targets.Add(obj);
        }

        focusObject = obj;
        WantsFocus = true;
        Open = true;
    }
}

[AttributeUsage(AttributeTargets.Method)]
class DebugButtonAttribute : Attribute
{
}