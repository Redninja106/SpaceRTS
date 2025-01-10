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
    public virtual ref Transform PreviousTransform => ref previousTransform;

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

    /// <summary>
    /// Moves the actor without interpolation.
    /// </summary>
    public void Teleport(Transform destination)
    {
        this.transform = destination;
        this.previousTransform = destination;
        this.interpolatedTransform = destination;
    }

    public override void Layout()
    {
        if (ImGui.CollapsingHeader("WorldActor"))
        {
            ImGui.Text("ID: " + ID);
            if (ImGui.TreeNode("Transform"))
            {
                Transform.Layout();
                ImGui.TreePop();
            }
        }
    }

    public override string ToString()
    {
        return base.ToString() + " (id: " + id + ")";
    }
}
