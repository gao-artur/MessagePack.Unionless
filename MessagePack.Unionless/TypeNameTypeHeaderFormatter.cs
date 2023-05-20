using System.Buffers;
using System.Reflection;
using System.Runtime.InteropServices;
using MessagePack.Unionless.Internal;

namespace MessagePack.Unionless;

public class TypeNameTypeHeaderFormatter : ITypeHeaderFormatter
{
    private static readonly ThreadsafeTypeKeyHashTable<byte[]> TypeToName = new();
    private static readonly AsymmetricKeyHashTable<byte[], ArraySegment<byte>, Type> NameToType = new(new StringArraySegmentByteAscymmetricEqualityComparer());

    // most likely base type implementations are located in a small group of assemblies
    // we try to reuse the same assemblies where implementations were found previously
    private readonly List<Assembly> _cachedAssemblies = new();

    public void Write(ref MessagePackWriter writer, Type type, UnionlessMessagePackSerializerOptions options)
    {
        if (!TypeToName.TryGetValue(type, out var typeName))
        {
            var fullName = type.FullName
                ?? throw new MessagePackSerializationException($"Types without names are not supported: {type}");

            typeName = StringEncoding.UTF8.GetBytes(fullName);
            TypeToName.TryAdd(type, typeName);
        }

        writer.WriteString(typeName);
    }

    public Type Read(ref MessagePackReader reader, UnionlessMessagePackSerializerOptions options)
    {
        var typeName = reader.ReadStringSequence() 
            ?? throw new MessagePackSerializationException("Can't read the type name");

        byte[]? rented = null;

        try
        {
            if (!typeName.IsSingleSegment || !MemoryMarshal.TryGetArray(typeName.First, out var typeNameArraySegment))
            {
                rented = ArrayPool<byte>.Shared.Rent((int)typeName.Length);
                typeName.CopyTo(rented);
                typeNameArraySegment = new ArraySegment<byte>(rented, 0, (int)typeName.Length);
            }

            return GetTypeByName(typeNameArraySegment);
        }
        finally
        {
            if (rented != null)
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    private Type GetTypeByName(ArraySegment<byte> typeName)
    {
        if (!NameToType.TryGetValue(typeName, out var type))
        {
            var buffer = new byte[typeName.Count];
            Buffer.BlockCopy(typeName.Array!, typeName.Offset, buffer, 0, buffer.Length);
            var name = StringEncoding.UTF8.GetString(buffer);

            // try to use assembly where the previous types were found
            foreach (var assembly in _cachedAssemblies)
            {
                type = assembly.GetType(name, throwOnError: false);
                if (type != null)
                {
                    NameToType.TryAdd(buffer, type);
                    return type;
                }
            }

            // iterate all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()
                         .Where(a => !_cachedAssemblies.Contains(a)))
            {
                type = assembly.GetType(name, throwOnError: false);
                if (type != null)
                {
                    NameToType.TryAdd(buffer, type);
                    _cachedAssemblies.Add(assembly);
                    return type;
                }
            }

            throw new MessagePackSerializationException($"Can't find type '{name}' in loaded assemblies");
        }

        return type;
    }
}