using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class NetworkMap
{
    private Dictionary<int, Actor> actorMap = [];
    private Dictionary<Actor, int> idMap = [];

    public NetworkMap()
    {

    }

    public Actor GetActor(int id)
    {
        return actorMap[id];
    }
    public int GetID(Actor actor)
    {
        return idMap[actor];
    }

    public void Register(Actor actor, int id)
    {
        actorMap.Add(id, actor);
        idMap.Add(actor, id);
    }

    public void Remove(Actor actor)
    {
        if (idMap.Remove(actor, out int index))
        {
            actorMap.Remove(index);
        }
    }

    public void Clear()
    {
        actorMap.Clear();
        idMap.Clear();
    }

}
