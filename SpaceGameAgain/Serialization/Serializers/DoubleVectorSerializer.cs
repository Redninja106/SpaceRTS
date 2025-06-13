using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Serialization.Serializers;
internal class DoubleVectorSerializer : Serializer
{
    public override object Deserialize(BinaryReader reader)
    {
        double x = reader.ReadDouble();
        double y = reader.ReadDouble();
        return new DoubleVector(x, y);
    }

    public override void Serialize(BinaryWriter writer, object value)
    {
        DoubleVector vector = (DoubleVector)value;
        writer.Write(vector.X);
        writer.Write(vector.Y);
    }
}
