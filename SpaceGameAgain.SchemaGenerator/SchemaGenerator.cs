using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SpaceGame.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGameAgain.SchemaGenerator;
internal class SchemaGenerator
{
    private DefaultContractResolver contractResolver = new();
    private Dictionary<Type, List<string>> prototypesByClass;

    public SchemaGenerator(Dictionary<Type, List<string>> prototypesByClass)
    {
        this.prototypesByClass = prototypesByClass;
    }

    public JObject GenerateSchema(Type type)
    {
        return GenerateValueSchema(type, true);
    }

    private JObject GenerateValueSchema(Type type, bool rootLevel = false)
    {
        if (!rootLevel && type.IsSubclassOf(typeof(Prototype)))
        {
            prototypesByClass.TryGetValue(type, out List<string>? values);
            return new JObject()
            {
                {
                    "enum",
                    new JArray(values?.Select(name => new JValue(name))?.ToArray() ?? [])
                }
            };
        }

        JsonContract contract = contractResolver.ResolveContract(type);

        return contract switch
        {
            JsonObjectContract objectContract => GenerateObjectSchema(objectContract),
            JsonPrimitiveContract primitiveContract => GeneratePrimitiveSchema(primitiveContract),
            JsonArrayContract arrayContract => GenerateArraySchema(arrayContract),
            JsonDictionaryContract dictionaryContract => GenerateArraySchema(dictionaryContract),
            _ => throw new Exception("unknown contract " + contract.ToString()),
        };
    }

    private JObject GenerateArraySchema(JsonDictionaryContract dictionaryContract)
    {
        JObject result = new JObject();
        result.Add("type", new JValue("object"));
        return result;
    }

    private JObject GenerateArraySchema(JsonArrayContract arrayContract)
    {
        JObject result = new JObject();
        result.Add("type", new JValue("array"));
        return result;
    }

    private JObject GeneratePrimitiveSchema(JsonPrimitiveContract primitiveContract)
    {
        if (primitiveContract.UnderlyingType.IsSubclassOf(typeof(Enum)))
        {
            return GenerateEnumSchema(primitiveContract.UnderlyingType);
        }

        JObject result = new JObject
        {
            { "type", new JValue(GetTypeName(primitiveContract.UnderlyingType)) },
        };
        return result;

        static string GetTypeName(Type primitiveType)
        {
            if (primitiveType == typeof(float) || primitiveType == typeof(double))
                return "number";

            if (primitiveType == typeof(int))
                return "integer";

            if (primitiveType == typeof(string))
                return "string";

            if (primitiveType == typeof(bool))
                return "boolean";

            return "unknown";
        }
    }

    private JObject GenerateEnumSchema(Type underlyingType)
    {
        return new JObject()
        {
            { "enum", new JArray(Enum.GetNames(underlyingType).Select(JValue.CreateString)) }
        };
    }

    private JObject GenerateObjectSchema(JsonObjectContract contract)
    {
        JObject properties = new();

        foreach (JsonProperty property in contract.Properties)
        {
            if (property.AttributeProvider!.GetAttributes(typeof(JsonIgnoreAttribute), true).Any())
                continue;

            properties.Add(ToSnakeCase(property.PropertyName!), GenerateValueSchema(property.PropertyType!));
        }

        if (contract.UnderlyingType.IsSubclassOf(typeof(Prototype)))
        {
            properties.Add("prototype", new JObject() { { "const", contract.UnderlyingType.Name.ToString() } });
        }

        return new JObject()
        {
            { "type", "object" },
            { "properties", properties }
        };
    }

    static string ToSnakeCase(string name)
    {
        return char.ToLower(name[0]) + string.Concat(name[1..].Select(c => char.IsUpper(c) ? "_" + char.ToLower(c) : c.ToString()));
    }
}
