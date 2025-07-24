using ImGuiNET;
using SimulationFramework.Drawing;
using SpaceGame.Debugging;
using SpaceGame.Orders;
using SpaceGame.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;

[Serializable]
public abstract class Actor(Prototype prototype, GameWorld world, ulong id) : IInspectable
{
    internal GameWorld World { get; } = world;
    public virtual Prototype Prototype { get; } = prototype;

    // serialized manually -- necessary for reference handling
    private readonly ulong id = id;
    
    [Serialize] 
    private Transform transform = Transform.Default;
    private Transform previousTransform = Transform.Default;
    private Transform interpolatedTransform = Transform.Default;

    public virtual Transform InterpolatedTransform => interpolatedTransform;
    public virtual ref Transform Transform => ref transform;
    public virtual ref Transform PreviousTransform => ref previousTransform;

    public ulong ID => id;

    /// <summary>
    /// Called when the actor is just added to the world. Not called when the actor is deserialized.
    /// </summary>
    public virtual void InitializeActor()
    {
        this.Teleport(transform);
    }

    public virtual void Update(float tickProgress)
    {
        interpolatedTransform = Transform.Lerp(previousTransform, Transform, tickProgress);
    }

    /// <summary>
    /// The base implement Actor.Tick sets the previous transform of the actor to the current transform. Any 
    /// calculations that require PreviousTransform should happen before it is called and any calculations 
    /// that modify Transform should happen after it is called.
    /// </summary>
    public virtual void Tick()
    {
        previousTransform = Transform;
    }

    public virtual void Render(ICanvas canvas)
    {
    }

    public virtual void FinishDeserialization()
    {
    }

    /// <summary>
    /// Moves the actor without interpolation.
    /// </summary>
    public virtual void Teleport(Transform destination)
    {
        this.transform = destination;
        this.previousTransform = destination;
        this.interpolatedTransform = destination;
    }

    public virtual void DebugLayout()
    {
        ImGui.Text("Prototype: " + Prototype.Name);

        ImGui.Text("ID: " + ID);
        Transform.Layout();

        if (this.GetType().GetMethod("DebugLayout")?.DeclaringType == typeof(Actor))
        {
            DebugLayoutSubclass(this.GetType());
        }
    }

    private void DebugLayoutSubclass(Type type)
    {
        if (type == typeof(Actor))
            return;

        DebugLayoutSubclass(type.BaseType!);

        ImGui.SeparatorText(type.Name);
        ObjectViewer.ReflectionLayoutObjectFields(this, type);
    }

    public override string ToString()
    {
        return base.ToString() + " (id: " + id + ")";
    }

    internal PlanetRelativePosition GetPlanetRelativePosition() => World.GetPlanetRelativePosition(this.transform.Position);
}
