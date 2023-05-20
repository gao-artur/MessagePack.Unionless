using MessagePack.Formatters;
using MessagePack.Unionless.Benchmark.Events;

namespace MessagePack.Unionless.Benchmark;

public sealed class MyBaseTypeFormatterResolver : IFormatterResolver
{
    private static readonly UnionlessFormatter<EventBase> Formatter = new();
    private static readonly Type BaseType  = typeof(EventBase);

    private MyBaseTypeFormatterResolver()
    {
    }

    public static readonly IFormatterResolver Instance = new MyBaseTypeFormatterResolver();

    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        return typeof(T) == BaseType ? (IMessagePackFormatter<T>)Formatter : null;
    }
}