using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Serialization.Serializers;
internal class NullableSerializer : Serializer
{
    private Serializer objectSerializer;

    public NullableSerializer(Type objectType)
    {
        this.objectSerializer = Serializer.GetSerializer(objectType);
    }

    public override object? Deserialize(BinaryReader reader)
    {
        bool isNull = reader.ReadBoolean();
        if (isNull)
        {
            return null;
        }
        else
        {
            return objectSerializer.Deserialize(reader);
        }
    }

    public override void Serialize(BinaryWriter writer, object value)
    {
        writer.Write(value == null);
        if (value != null)
        {
            objectSerializer.Serialize(writer, value);
        }
    }
}
