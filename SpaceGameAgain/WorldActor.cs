using ImGuiNET;
using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;

internal abstract class WorldActor(WorldActorPrototype prototype, ulong id, Transform transform) : Actor(prototype)
{
    private readonly ulong id = id;
    private Transform transform = transform;
    private Transform previousTransform = transform;
    private Transform interpolatedTransform = transform;

    public virtual Transform InterpolatedTransform => interpolatedTransform;
    public virtual ref Transform Transform => ref transform;
    public ulong ID => id;

    public virtual void Update(float tickProgress)
    {
        interpolatedTransform = Transform.Lerp(previousTransform, Transform, tickProgress);
    }
    
    public virtual void Tick()
    {
        previousTransform = Transform;
    }

    public virtual void Render(ICanvas canvas)
    {
    }

    public override void Layout()
    {
        if (ImGui.CollapsingHeader("WorldActor"))
        {
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

    public override string ToString()
    {
        return base.ToString() + " (id: " + id + ")";
    }
}
