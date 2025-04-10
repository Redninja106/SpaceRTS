using Silk.NET.OpenGL;
using SpaceGame.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class NetworkSerializer
{
    public void Serialize(Actor actor, BinaryWriter writer)
    {
        DebugLog.Assert(!string.IsNullOrEmpty(actor.Prototype.Name));
        
        // writer.Write(0xDDBBCCAA);
        writer.Write(actor.Prototype.Name);
        actor.Serialize(writer);
    }

    public Actor Deserialize(BinaryReader reader)
    {
        // uint sig = reader.ReadUInt32();
        // DebugLog.Assert(sig == 0xDDBBCCAA);
        string prototypeName = reader.ReadString();
        DebugLog.Assert(!string.IsNullOrEmpty(prototypeName));
        Prototype prototype = Prototypes.Get(prototypeName);
        return prototype.Deserialize(reader);
    }

    public byte[] SerializeWithLengthPrefix(Actor actor)
    {
        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        writer.Write(0); // leave 4 bytes for packet size

        Serialize(actor, writer);

        // fill in those 4 bytes
        // NOTE the length includes the 4 bytes for itself
        int position = (int)ms.Position;
        ms.Position = 0;
        writer.Write(position);

        return ms.ToArray();
    }
}
