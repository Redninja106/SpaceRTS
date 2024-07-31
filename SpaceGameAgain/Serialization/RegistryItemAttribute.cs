using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Serialization;
[AttributeUsage(AttributeTargets.Class)]
internal class RegistryItemAttribute(string RegistryName) : Attribute
{
    public string RegistryName { get; } = RegistryName;
}
