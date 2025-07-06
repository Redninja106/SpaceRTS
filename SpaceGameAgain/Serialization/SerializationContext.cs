using SpaceGame.Serialization.Serializers;
using System.Reflection;

namespace SpaceGame.Serialization;

class SerializationContext
{
    private Dictionary<Type, Serializer> typeSerializers = [];
    private ActorReferenceSerializer actorReferenceSerializer;
    private PrototypeReferenceSerializer prototypeReferenceSerializer = new();

    public SerializationContext(GameWorld world)
    {
        actorReferenceSerializer = new(world);

        PrimitiveSerializers.Register(typeSerializers);

        typeSerializers.Add(typeof(DoubleVector), new DoubleVectorSerializer());
        typeSerializers.Add(typeof(Vector2), new Vector2Serializer());

        typeSerializers.Add(typeof(string), new StringSerializer());
        typeSerializers.Add(typeof(byte[]), new ByteArraySerializer());
    }

    public Serializer GetSerializer(Type type, bool nullable = false)
    {
        if (type == typeof(Actor) || type.IsSubclassOf(typeof(Actor)))
        {
            return actorReferenceSerializer;
        }

        if (nullable)
        {
            return new NullableSerializer(this, type);
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

    private Serializer CreateSerializer(Type type)
    {
        if (type.IsArray)
        {
            return new ArraySerializer(this, type, type.GetElementType()!);
        }

        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var genericArguments = type.GetGenericArguments();
            return new DictionarySerializer(this, type, genericArguments[0], genericArguments[1]);
        }

        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Queue<>))
        {
            return new QueueSerializer(this, type, type.GetGenericArguments()[0]);
        }

        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            return new ListSerializer(this, type, type.GetGenericArguments()[0]);
        }

        SerializableAttribute? serializableAttribute = type.GetCustomAttribute<SerializableAttribute>();
        if (serializableAttribute != null)
        {
            if (serializableAttribute.Abstract)
            {
                return new AbstractClassSerializer(this, type);
            }
            else
            {
                return new ObjectSerializer(this, type);
            }
        }

        throw new InvalidOperationException($"{type} is not serializable!");
    }
}
