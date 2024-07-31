using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Serialization;
internal class Registry<T>
    where T : class
{
    private ImmutableArray<T> entries;
    private ImmutableDictionary<T, int> idMap;

    public Registry(ImmutableArray<T> entries)
    {
        this.entries = entries;

        var idMap = new Dictionary<T, int>();
        for (int i = 0; i < entries.Length; i++)
        {
            idMap[entries[i]] = i;
        }
        this.idMap = idMap.ToImmutableDictionary();
    }

    public T Get(int id)
    {
        return entries[id];
    }

    public int GetID(T entry)
    {
        return idMap[entry];
    }
}