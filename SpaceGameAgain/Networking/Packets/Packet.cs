using SpaceGame.Commands;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking.Packets;

internal class Packet
{
    private static Dictionary<string, Type> packetTypes = [];

    static Packet()
    {
        foreach (var type in Assembly.GetExecutingAssembly().DefinedTypes)
        {
            if (type.GetCustomAttribute<PacketIDAttribute>() is PacketIDAttribute attr)
            {
                packetTypes.Add(attr.ID, type);
            }
        }
    }

    public static void Serialize(Packet packet, Stream stream)
    {
        PacketIDAttribute? idAttribute = packet.GetType().GetCustomAttribute<PacketIDAttribute>();
        if (idAttribute is null)
        {
            throw new($"packet of type {packet.GetType().Name} has no PacketID attribute!");
        }
        string id = idAttribute.ID;

        if (id.Length != 4)
        {
            throw new($"packet of type {packet.GetType().Name} must have an id with length 4!");
        }

        stream.WriteByte((byte)id[0]);
        stream.WriteByte((byte)id[1]);
        stream.WriteByte((byte)id[2]);
        stream.WriteByte((byte)id[3]);

        SerializeFields(packet, packet.GetType(), stream);
    }

    private static void SerializeFields(object? obj, Type type, Stream stream)
    {
        switch (obj)
        {
            case long l:
                WriteValue(stream, ref l);
                return;
            case int i:
                WriteValue(stream, ref i);
                return;
            case float f:
                WriteValue(stream, ref f);
                return;
            case Vector2 v2:
                WriteValue(stream, ref v2);
                return;
            case HexCoordinate hex:
                WriteValue(stream, ref hex);
                return;
            case string s:
                int length = s.Length;
                WriteValue(stream, ref length);
                stream.Write(Encoding.UTF8.GetBytes(s));
                return;
            case Actor actor:
                int actorID = actor.ID;
                WriteValue(stream, ref actorID);
                return;
            default:
                break;
        }

        if (type.GetCustomAttribute<PacketBaseClassAttribute>() != null)
        {
            string typeName = obj!.GetType().Name;
            int length = typeName.Length;
            WriteValue(stream, ref length);
            stream.Write(Encoding.UTF8.GetBytes(typeName));
            SerializeFields(obj, obj.GetType(), stream);
            return;
        }

        if (type.IsPrimitive || type.IsValueType)
        {
            throw new("unsupported primitive or value type" + type.Name);
        }

        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var value = field.GetValue(obj)!;
            if (value is Array array)
            {
                int length = array.Length;
                WriteValue(stream, ref length);
                var elemType = field.FieldType.GetElementType()!;
                for (int i = 0; i < array.Length; i++)
                {
                    SerializeFields(array.GetValue(i)!, elemType, stream);
                }
            }
            else
            {
                SerializeFields(value, field.FieldType, stream);
            }
        }
    }

    private static unsafe void WriteValue<T>(Stream stream, ref T instance) where T : unmanaged
    {
        stream.Write(new Span<byte>(Unsafe.AsPointer(ref instance), Unsafe.SizeOf<T>()));
    }

    private static unsafe T ReadValue<T>(Stream stream) where T : unmanaged
    {
        T result = default;
        var span = new Span<byte>(Unsafe.AsPointer(ref result), Unsafe.SizeOf<T>());
        stream.Read(span);
        return result;
    }

    public static Packet Deserialize(Stream stream)
    {
        char c0 = (char)stream.ReadByte();
        char c1 = (char)stream.ReadByte();
        char c2 = (char)stream.ReadByte();
        char c3 = (char)stream.ReadByte();

        var id = new string([c0, c1, c2, c3]);

        var type = packetTypes[id];

        return (Packet)DeserializeFields(type, stream);
    }

    private static object DeserializeFields(Type type, Stream stream)
    {
        if (type == typeof(long))
        {
            return ReadValue<long>(stream);
        }
        if (type == typeof(int))
        {
            return ReadValue<int>(stream);
        }
        if (type == typeof(float))
        {
            return ReadValue<float>(stream);
        }
        if (type == typeof(Vector2))
        {
            return ReadValue<Vector2>(stream);
        }
        if (type == typeof(HexCoordinate))
        {
            return ReadValue<HexCoordinate>(stream);
        }
        if (type == typeof(string))
        {
            int length = ReadValue<int>(stream);
            byte[] buffer = new byte[length];
            stream.Read(buffer);
            return Encoding.UTF8.GetString(buffer);
        }
        if (type.IsSubclassOf(typeof(Actor)))
        {
            int id = ReadValue<int>(stream);
            return World.NetworkMap.GetActor(id);
            
        }

        if (type.GetCustomAttribute<PacketBaseClassAttribute>() != null)
        {
            int length = ReadValue<int>(stream);
            byte[] buffer = new byte[length];
            stream.Read(buffer);
            var typeName = Encoding.UTF8.GetString(buffer);
            var actualType = Assembly.GetExecutingAssembly().DefinedTypes.Single(t => t.Name == typeName)!;
            return DeserializeFields(actualType, stream);
        }

        if (type.IsPrimitive || type.IsValueType)
        {
            throw new("unsupported primitive or value type" + type.Name);
        }
        
        if (type.IsArray)
        {
            Type elementType = type.GetElementType() ?? throw new();
            int length = ReadValue<int>(stream);
            var array = Array.CreateInstance(elementType, length);
            for (int i = 0; i < length; i++)
            {
                array.SetValue(DeserializeFields(elementType, stream), i);
            }
            return array;
        }

        var instance = Activator.CreateInstance(type) ?? throw new("Could not create instance of type " + type.Name);
        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            field.SetValue(instance, DeserializeFields(field.FieldType, stream));
        }
        return instance;
    }
}