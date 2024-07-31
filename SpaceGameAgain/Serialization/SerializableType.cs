using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Serialization;
internal static class SerializableType
{
    private static Dictionary<string, Type> nameMap = [];

    static SerializableType()
    {
        foreach (var type in Assembly.GetExecutingAssembly().DefinedTypes)
        {
            if (type.GetCustomAttribute<SerializableTypeAttribute>() == null)
                continue;

            nameMap.Add(type.Name, type);
        }
    }

    public static Type FromName(string name)
    {
        return nameMap[name];
    }
}
