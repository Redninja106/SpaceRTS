using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class NetworkMap
{
    private Dictionary<int, Actor> map = [];

    public NetworkMap()
    {

    }

    public Actor GetActor(int id)
    {
        return map[id];
    }

    public void Register(int id, Actor actor)
    {
        map.Add(id, actor);
    }
}
