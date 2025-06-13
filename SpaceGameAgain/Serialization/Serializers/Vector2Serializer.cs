using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Serialization.Serializers;
internal class Vector2Serializer : Serializer
{
    public override object Deserialize(BinaryReader reader)
    {
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        return new Vector2(x, y);
    }

    public override void Serialize(BinaryWriter writer, object value)
    {
        Vector2 vector = (Vector2)value;
        writer.Write(vector.X);
        writer.Write(vector.Y);
    }
}
