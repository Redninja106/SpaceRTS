using SpaceGame.Serialization.Serializers;
using System.Reflection;

namespace SpaceGame.Serialization;

abstract class Serializer
{
    private static Dictionary<Type, Serializer> typeSerializers = [];
    private static ActorReferenceSerializer actorReferenceSerializer = new(Program.World);
    private static PrototypeReferenceSerializer prototypeReferenceSerializer = new PrototypeReferenceSerializer();

    public abstract void Serialize(BinaryWriter writer, object value);
    public abstract object? Deserialize(BinaryReader reader);

    static Serializer()
    {
        PrimitiveSerializers.Register(typeSerializers);

        typeSerializers.Add(typeof(DoubleVector), new DoubleVectorSerializer());
        typeSerializers.Add(typeof(Vector2), new Vector2Serializer());
        
        typeSerializers.Add(typeof(string), new StringSerializer());
        typeSerializers.Add(typeof(byte[]), new ByteArraySerializer());
    }

    public static Serializer GetSerializer(Type type, bool nullable = false)
    {
        if (type == typeof(Actor) || type.IsSubclassOf(typeof(Actor)))
        {
            return actorReferenceSerializer;
        }

        if (nullable)
        {
            return new NullableSerializer(type);
        }

        if (type == typeof(Prototype) || type.IsSubclassOf(typeof(Prototype)))
        {
            return prototypeReferenceSerializer;
        }

        if (typeSerializers.TryGetValue(type, out Serializer? value))
        {
            return value;
        }

        Serializer result = CreateSerializer(type);
        typeSerializers.Add(type, result);
        return result;
    }

    private static Serializer CreateSerializer(Type type)
    {
        if (type.IsArray)
        {
            return new ArraySerializer(type, type.GetElementType()!);
        }

        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var genericArguments = type.GetGenericArguments();
            return new DictionarySerializer(type, genericArguments[0], genericArguments[1]);
        }

        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Queue<>))
        {
            return new QueueSerializer(type, type.GetGenericArguments()[0]);
        }

        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            return new ListSerializer(type, type.GetGenericArguments()[0]);
        }

        SerializableAttribute? serializableAttribute = type.GetCustomAttribute<SerializableAttribute>();
        if (serializableAttribute != null)
        {
            if (serializableAttribute.Abstract)
            {
                return new AbstractClassSerializer(type);
            }
            else
            {
                return new ObjectSerializer(type);
            }
        }

        throw new InvalidOperationException($"{type} is not serializable!");
    }
}
