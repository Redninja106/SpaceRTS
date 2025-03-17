using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpaceGame;
internal abstract class Prototype
{
    [JsonInclude]
    public string Name { get; private set; }

    public abstract Actor Deserialize(BinaryReader reader);

    public virtual void InitializePrototype()
    {
    }

    public override string ToString()
    {
        return $"{Name} ({GetType().Name})";
    }
}
