using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarketplaceBackend.Utils.JsonConverters;

public class NullToEmptyEnumerableConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert == typeof(string)) return false;
        return typeof(IEnumerable).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert.IsArray)
        {
            var elementType = typeToConvert.GetElementType()!;
            var converterType = typeof(NullToEmptyArrayConverter<>).MakeGenericType(elementType);
            return (JsonConverter?)Activator.CreateInstance(converterType)!;
        }

        if (typeToConvert.IsGenericType)
        {
            var generic = typeToConvert.GetGenericTypeDefinition();
            if (generic == typeof(IEnumerable<>) || generic == typeof(ICollection<>) || generic == typeof(IList<>) || generic == typeof(List<>))
            {
                var elementType = typeToConvert.GetGenericArguments()[0];
                var converterType = typeof(NullToEmptyListConverter<>).MakeGenericType(elementType);
                return (JsonConverter?)Activator.CreateInstance(converterType)!;
            }
        }

        return null;
    }
}

public class NullToEmptyArrayConverter<T> : JsonConverter<T[]>
{
    public override T[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<T[]>(ref reader, options) ?? Array.Empty<T>();
    }

    public override void Write(Utf8JsonWriter writer, T[]? value, JsonSerializerOptions options)
    {
        if (value == null) value = Array.Empty<T>();
        JsonSerializer.Serialize(writer, value, options);
    }
}

public class NullToEmptyListConverter<T> : JsonConverter<List<T>>
{
    public override List<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<List<T>>(ref reader, options) ?? new List<T>();
    }

    public override void Write(Utf8JsonWriter writer, List<T>? value, JsonSerializerOptions options)
    {
        if (value == null) value = new List<T>();
        JsonSerializer.Serialize(writer, value, options);
    }
}
