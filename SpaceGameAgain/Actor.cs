using ImGuiNET;
using SpaceGame.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal abstract class Actor(Prototype prototype) : IInspectable
{
    public virtual Prototype Prototype { get; } = prototype;

    public virtual void DebugLayout()
    {
        ImGui.Text("Prototype: " + Prototype.Name);
    }

    public abstract void Serialize(BinaryWriter writer);

    public virtual void FinalizeDeserialization()
    {
    }

}
