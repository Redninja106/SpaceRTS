using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpaceGame.Data;

// [JsonObject(NamingStrategyType = typeof(KebabCaseNamingStrategy)]
public abstract class Prototype
{
    [JsonInclude]
    public string Name { get; private set; }

    public abstract PrototypeObject Deserialize(BinaryReader reader);

    public virtual void InitializePrototype()
    {
    }

    public override string ToString()
    {
        return $"{Name} ({GetType().Name})";
    }

    [DebugButton]
    public void Reload()
    {
        Prototypes.ReloadPrototype(this);
    }
}
