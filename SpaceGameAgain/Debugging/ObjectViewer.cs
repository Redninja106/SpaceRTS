using ImGuiNET;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using static Program;
using System.Reflection.Emit;

namespace SpaceGame.Debugging;

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
            foreach (var target in targets.ToArray())
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
                    try
                    {
                        if (target is IInspectable inspectable)
                        {
                            inspectable.DebugLayout();
                        }
                        else
                        {
                            ReflectionLayoutObjectFields(target);
                        }
                    }
                    catch (Exception ex)
                    {
                        ImGui.Text(ex.ToString());
                    }

                    ImGui.EndTabItem();
                }
                if (!open)
                {
                    targets.Remove(target);

                    if (targets.Count == 0)
                    {
                        Open = false;
                    }
                }
            }

            ImGui.EndTabBar();
        }
    }

    public static void LayoutActorLink(Actor actor, string? label = null)
    {
        bool missing = !Program.World.Actors.ContainsKey(actor.ID);
        if (missing)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, 0xFF0000FF);
        }
        if (ImGui.Selectable((label ?? actor.ToString()) + (missing ? " (not registered)" : "")))
        {
            DebugMenu.ViewObject(actor);
        }
        if (missing)
        {
            ImGui.PopStyleColor();
        }
    }

    public static object? ReflectionLayoutObject(string label, object? obj, bool isReadonly)
    {
        //if ((obj?.GetType()?.IsConstructedGenericType ?? false) && obj.GetType().GetGenericTypeDefinition() == typeof(ActorReference<>))
        //{
        //    ReflectionLayoutObject(label, obj.GetType().GetField("actor", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(obj), isReadonly);
        //    return obj;
        //}

        if (obj != null)
        {
            label += $" ({FormatTypeName(obj.GetType())})";
        }

        switch (obj)
        {
            case Prototype proto:
                if (ImGui.Selectable($"{label}: {proto.Name} ({proto.GetType().Name})"))
                {
                    DebugMenu.ViewObject(proto);
                }
                return proto;
            case Actor actor:
                LayoutActorLink(actor, label);
                return actor;
            case IInspectable inspectable:
                if (ImGui.TreeNode(label))
                {
                    inspectable.DebugLayout();
                    ImGui.TreePop();
                }
                return inspectable;
            case float or Vector2 or DoubleVector or int or bool or string or Enum:
            case object when obj.GetType().IsPrimitive:
                if (isReadonly)
                {
                    ImGui.BeginDisabled();
                }
                object? result = LayoutPrimitiveObject(label, obj);
                if (isReadonly)
                {
                    ImGui.EndDisabled();
                }
                return result;
            case IEnumerable enumerable:
                if (ImGui.TreeNode(label))
                {
                    int i = 0;
                    foreach (var element in enumerable)
                    {
                        ReflectionLayoutObject(i++.ToString(), element, element.GetType().IsValueType);
                    }

                    if (i == 0)
                    {
                        ImGui.TextDisabled("(empty)");
                    }
                    ImGui.TreePop();
                }
                return enumerable;
            case null:
            case object:
                if (ImGui.TreeNode(label))
                {
                    if (obj == null)
                    {
                        ImGui.TextDisabled("(null)");
                    }
                    else
                    {
                        ReflectionLayoutObjectFields(obj);
                    }
                    ImGui.TreePop();
                }
                return obj;
        }
    }

    private static string FormatTypeName(Type type)
    {
        if (type.IsConstructedGenericType)
        {
            return $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GetGenericArguments().Select(FormatTypeName))}>";
        }

        return type.Name;
    }

    private static object? LayoutPrimitiveObject(string label, object? obj)
    {
        switch (obj)
        {
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
            case Enum e:
                Array values = Enum.GetValues(e.GetType());
                int current = Array.IndexOf(values, e);
                string opts = (string)values.Cast<object>().Aggregate((a, b) => a.ToString() + "\0" + b.ToString());

                if (ImGui.Combo(label, ref current, opts))
                {
                    return values.GetValue(current);
                }
                return e;
            case object when obj.GetType().IsPrimitive:
                string objValue = obj?.ToString() ?? "null";
                ImGui.InputText(label, ref objValue, (uint)objValue.Length);
                return obj;
            default:
                throw new UnreachableException();
        }
    }

    public static void ReflectionLayoutObjectFields(object obj, Type? type = null)
    {
        BindingFlags bindFlags = BindingFlags.Public | BindingFlags.Instance;
        if (type == null)
        {
            type = obj.GetType();
        }
        else
        {
            bindFlags |= BindingFlags.DeclaredOnly;
        }
        foreach (var member in type.GetMembers(bindFlags))
        {
            if (member.GetCustomAttribute<DebugIgnoreAttribute>() != null)
            {
                continue;
            }

            object? value;
            switch (member)
            {
                case FieldInfo field when !field.Name.Contains('<'):
                    value = field.GetValue(obj);
                    value = ReflectionLayoutObject(field.Name, value, !field.Attributes.HasFlag(FieldAttributes.InitOnly));
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

    public void View(object? obj)
    {
        if (obj != null && !targets.Contains(obj))
        {
            targets.Add(obj);
        }

        focusObject = obj;
        WantsFocus = true;
        Open = true;
    }
}
