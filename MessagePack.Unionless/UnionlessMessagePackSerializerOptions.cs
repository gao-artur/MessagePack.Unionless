namespace MessagePack.Unionless;

public class UnionlessMessagePackSerializerOptions : MessagePackSerializerOptions
{
    public UnionlessMessagePackSerializerOptions(IFormatterResolver resolver)
        : base(resolver)
    {
    }

    public UnionlessMessagePackSerializerOptions(MessagePackSerializerOptions copyFrom)
        : base(copyFrom)
    {
        if (copyFrom is UnionlessMessagePackSerializerOptions eventOptions)
        {
            TypeHeaderFormatter = eventOptions.TypeHeaderFormatter;
        }
    }

    public required ITypeHeaderFormatter TypeHeaderFormatter { get; init; }
}