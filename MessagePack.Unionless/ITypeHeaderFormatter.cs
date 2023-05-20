namespace MessagePack.Unionless;

public interface ITypeHeaderFormatter
{
    void Write(ref MessagePackWriter writer, Type type, UnionlessMessagePackSerializerOptions options);

    Type Read(ref MessagePackReader reader, UnionlessMessagePackSerializerOptions options);
}