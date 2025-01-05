using SpaceGame.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class NetworkSerializer
{
    public void Serialize(Actor actor, BinaryWriter writer)
    {
        writer.Write(actor.Prototype.Name);
        actor.Serialize(writer);
    }

    public Actor Deserialize(BinaryReader reader)
    {
        string prototypeName = reader.ReadString();
        Prototype prototype = Prototypes.Get(prototypeName);
        return prototype.Deserialize(reader);
    }

}
