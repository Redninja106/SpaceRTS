using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Serialization.Serializers;
internal class DoubleSerializer : Serializer
{
    public override object Deserialize(BinaryReader reader)
    {
        return reader.ReadDouble();
    }

    public override void Serialize(BinaryWriter writer, object value)
    {
        writer.Write((double)value);
    }
}
