using System.Linq.Expressions;
using System.Reflection;
using MessagePack.Formatters;
using MessagePack.Unionless.Internal;

namespace MessagePack.Unionless;

// T is always base type
public class UnionlessFormatter<T> : EventsFormatter, IMessagePackFormatter<T>
{
    public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        if (options is not UnionlessMessagePackSerializerOptions eventOptions)
        {
            throw new MessagePackSerializationException($"Options of type '{nameof(UnionlessMessagePackSerializerOptions)}' expected");
        }

        writer.WriteArrayHeader(2);

        var actualType = value.GetType();

        eventOptions.TypeHeaderFormatter.Write(ref writer, actualType, eventOptions);

        SerializeInternal(actualType, ref writer, value, options);
    }

    public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return default!;
        }

        options.Security.DepthStep(ref reader);

        try
        {
            var count = reader.ReadArrayHeader();

            if (count != 2)
            {
                throw new MessagePackSerializationException($"A two element array expected. Actual count: '{count}'");
            }

            if (options is not UnionlessMessagePackSerializerOptions eventOptions)
            {
                throw new MessagePackSerializationException($"Options of type '{nameof(UnionlessMessagePackSerializerOptions)}' expected");
            }

            var type = eventOptions.TypeHeaderFormatter.Read(ref reader, eventOptions);

            return DeserializeInternal<T>(type, ref reader, options);
        }
        finally
        {
            reader.Depth--;
        }
    }
}

// use base non-generic type to store static fields
public abstract class EventsFormatter
{
    private delegate void SerializeMethod(object formatter, ref MessagePackWriter writer, object value, MessagePackSerializerOptions options);
    private delegate object DeserializeMethod(object formatter, ref MessagePackReader reader, MessagePackSerializerOptions options);

    private static readonly ThreadsafeTypeKeyHashTable<SerializeMethod> Serializers = new();
    private static readonly ThreadsafeTypeKeyHashTable<DeserializeMethod> Deserializers = new();

    protected static void SerializeInternal<T>(Type type, ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
    {
        var formatter = options.Resolver.GetFormatterDynamic(type)
            ?? throw new MessagePackSerializationException($"Can't determine formatter for type: {type.FullName}");

        var serializeMethod = Serializers.GetOrAdd(type, t =>
        {
            var formatterType = typeof(IMessagePackFormatter<>).MakeGenericType(t);
            var param0 = Expression.Parameter(typeof(object), "formatter");
            var param1 = Expression.Parameter(typeof(MessagePackWriter).MakeByRefType(), "writer");
            var param2 = Expression.Parameter(typeof(object), "value");
            var param3 = Expression.Parameter(typeof(MessagePackSerializerOptions), "options");

            var serializeMethodInfo = formatterType.GetRuntimeMethod("Serialize", new[] { typeof(MessagePackWriter).MakeByRefType(), t, typeof(MessagePackSerializerOptions) })!;

            var body = Expression.Call(
                Expression.Convert(param0, formatterType),
                serializeMethodInfo,
                param1,
                t.GetTypeInfo().IsValueType ? Expression.Unbox(param2, t) : Expression.Convert(param2, t),
                param3);

            return Expression.Lambda<SerializeMethod>(body, param0, param1, param2, param3).Compile();
        });

        serializeMethod(formatter, ref writer, value!, options);
    }

    protected static T DeserializeInternal<T>(Type type, ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        options.ThrowIfDeserializingTypeIsDisallowed(type);

        var formatter = options.Resolver.GetFormatterDynamic(type)
            ?? throw new MessagePackSerializationException($"Can't determine formatter for type: {type.FullName}");

        var deserializeMethod = Deserializers.GetOrAdd(type, t =>
        {
            var formatterType = typeof(IMessagePackFormatter<>).MakeGenericType(t);
            var param0 = Expression.Parameter(typeof(object), "formatter");
            var param1 = Expression.Parameter(typeof(MessagePackReader).MakeByRefType(), "reader");
            var param2 = Expression.Parameter(typeof(MessagePackSerializerOptions), "options");

            var deserializeMethodInfo = formatterType.GetRuntimeMethod("Deserialize", new[] { typeof(MessagePackReader).MakeByRefType(), typeof(MessagePackSerializerOptions) })!;

            var deserialize = Expression.Call(
                Expression.Convert(param0, formatterType),
                deserializeMethodInfo,
                param1,
                param2);

            Expression body = deserialize;
            if (t.GetTypeInfo().IsValueType)
            {
                body = Expression.Convert(deserialize, typeof(object));
            }

            return Expression.Lambda<DeserializeMethod>(body, param0, param1, param2).Compile();
        });

        return (T)deserializeMethod(formatter, ref reader, options);
    }
}
