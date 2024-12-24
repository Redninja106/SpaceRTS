using ImGuiNET;
using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal abstract class Actor(Prototype prototype, ulong id, Transform transform) : IInspectable
{
    public virtual Prototype Prototype { get; } = prototype;

    private readonly ulong id = id;
    private Transform transform = transform;

    public virtual ref Transform Transform => ref transform;
    public ulong ID => id;

    public virtual void Update()
    {
    }

    public virtual void Render(ICanvas canvas)
    {
    }

    public virtual void Layout()
    {
        if (ImGui.CollapsingHeader("Actor"))
        {
            ImGui.Text("Prototype: " + Prototype.Name);
            ImGui.Text("ID: " + ID);
            if (ImGui.TreeNode("Transform"))
            {
                ImGui.DragFloat2("Position", ref this.transform.Position);
                ImGui.SliderAngle("Rotation", ref this.transform.Rotation);
                ImGui.DragFloat2("Scale", ref this.transform.Position);
                ImGui.TreePop();
            }
        }
    }

    public abstract void Serialize(BinaryWriter writer);

    public override string ToString()
    {
        return base.ToString() + " (id: " + id + ")";
    }

    public virtual void FinalizeDeserialization()
    {
    }
}
