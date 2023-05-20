namespace MessagePack.Unionless;

public class TypeIdTypeHeaderFormatter : ITypeHeaderFormatter
{
    private readonly IReadOnlyDictionary<Type, int> _typeToIdMap;
    private readonly IReadOnlyDictionary<int, Type> _idToTypeMap;

    public TypeIdTypeHeaderFormatter(IReadOnlyDictionary<Type, int> typeToIdMap, IReadOnlyDictionary<int, Type> idToTypeMap)
    {
        _typeToIdMap = typeToIdMap;
        _idToTypeMap = idToTypeMap;
    }

    public void Write(ref MessagePackWriter writer, Type type, UnionlessMessagePackSerializerOptions options)
    {
        if (!_typeToIdMap.TryGetValue(type, out var typeId))
        {
            throw new MessagePackSerializationException($"Can't find an id for type '{type.FullName}'");
        }

        writer.Write(typeId);
    }

    public Type Read(ref MessagePackReader reader, UnionlessMessagePackSerializerOptions options)
    {
        var typeId = reader.ReadInt32();

        if (!_idToTypeMap.TryGetValue(typeId, out var type))
        {
            throw new MessagePackSerializationException($"Can't find the type for id '{typeId}'");
        }

        return type;
    }
}