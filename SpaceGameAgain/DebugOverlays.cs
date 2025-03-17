using ImGuiNET;
using System.Reflection;

namespace SpaceGame;

static class DebugOverlays
{
    private static List<DebugOverlayInfo> overlays = [];

    public static void Register()
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var members = type.FindMembers(
                MemberTypes.Method | MemberTypes.Field,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                (m, _) => m.GetCustomAttribute<DebugOverlayAttribute>() != null,
                null
                );

            foreach (var member in members)
            {
                switch (member)
                {
                    case MethodInfo method:
                        overlays.Add(new DebugOverlayInfo()
                        {
                            Name = member.Name,
                            Enabled = false,
                            Tick = method.CreateDelegate<Action>(),
                        });
                        break;
                    case FieldInfo field:
                        overlays.Add(new DebugOverlayInfo()
                        {
                            Name = member.Name,
                            Enabled = false,
                            field = field,
                        });
                        break;
                    default:
                        break;
                }
                
            }
        }
    }

    public static void Layout()
    {
        foreach (var overlay in overlays)
        {
            if (ImGui.Checkbox(overlay.Name, ref overlay.Enabled))
            {
                overlay.field?.SetValue(null, overlay.Enabled);
            }
        }
    }

    public static void Tick()
    {
        foreach (var overlay in overlays)
        {
            if (overlay.Enabled)
            {
                overlay.Tick?.Invoke();
            }
        }
    }

    private class DebugOverlayInfo
    {
        public required string Name;
        public bool Enabled;
        public Action? Tick;
        public FieldInfo? field;
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
class DebugOverlayAttribute : Attribute
{
}