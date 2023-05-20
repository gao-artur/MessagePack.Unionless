using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace MessagePack.Unionless;

public class TypeNameTypeHeaderFormatter : ITypeHeaderFormatter
{
    private static readonly ConcurrentDictionary<Type, byte[]> TypeToName = new();
    private static readonly ConcurrentDictionary<string, Type> NameToType = new();

    // most likely base type implementations are located in a small group of assemblies
    // we try to reuse the same assemblies where implementations were found previously
    private readonly List<Assembly> _cachedAssemblies = new();

    public void Write(ref MessagePackWriter writer, Type type, UnionlessMessagePackSerializerOptions options)
    {
        var bytes = TypeToName.GetOrAdd(type, t =>
        {
            var typeName = t.FullName
                ?? throw new MessagePackSerializationException($"Types without names are not supported: {t}");

            return Encoding.UTF8.GetBytes(typeName);
        });

        writer.WriteString(bytes);
    }

    public Type Read(ref MessagePackReader reader, UnionlessMessagePackSerializerOptions options)
    {
        var typeName = reader.ReadString()
            ?? throw new MessagePackSerializationException("Can't read the type name");

        var type = NameToType.GetOrAdd(typeName, name =>
        {
            // try to use assembly where the previous types were found
            foreach (var assembly in _cachedAssemblies)
            {
                var foundType = assembly.GetType(name, throwOnError: false);
                if (foundType != null)
                {
                    return foundType;
                }
            }

            // iterate all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()
                         .Where(a => !_cachedAssemblies.Contains(a)))
            {
                var foundType = assembly.GetType(name, throwOnError: false);
                if (foundType != null)
                {
                    _cachedAssemblies.Add(assembly);
                    return foundType;
                }
            }

            throw new MessagePackSerializationException($"Can't find type '{name}' in loaded assemblies");
        });

        return type;
    }
}