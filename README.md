# MessagePack.Unionless

This implementation is inspired by Phylum123's implementation of
[PolymorphicMessagePack](https://github.com/Phylum123/PolymorphicMessagePack) and relies heavily on [TypelessFormatter](https://github.com/neuecc/MessagePack-CSharp/blob/v2.5.108/src/MessagePack.UnityClient/Assets/Scripts/MessagePack/Formatters/TypelessFormatter.cs) implementation.

## How to use

Implement `IFormatterResolver` for your base type

```c#
public sealed class MyBaseTypeFormatterResolver : IFormatterResolver
{
    private static readonly UnionlessFormatter<IMyBaseType> Formatter = new();

    private MyBaseTypeFormatterResolver()
    {
    }

    public static IFormatterResolver Instance { get; } = 
        new MyBaseTypeFormatterResolver();

    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        return typeof(T) == typeof(IMyBaseType)
            ? (IMessagePackFormatter<T>)Formatter
            : null;
    }
}
```

Create the `UnionlessMessagePackSerializerOptions` with the previously created formatter resolver as the first in the chain

```c#
var options = new UnionlessMessagePackSerializerOptions(
    MessagePackSerializer.DefaultOptions
        .WithResolver(CompositeResolver.Create(
            MyBaseTypeFormatterResolver.Instance,  // <== your resolver should be first
            StandardResolver.Instance)))
{
    TypeHeaderFormatter = new TypeIdTypeHeaderFormatter(
        new Dictionary<Type, int>
        {
            [typeof(DerivedClass)] = 0,
            [typeof(DerivedStruct)] = 1
        }
    )
};
```

It's up to you how to fill the type-to-id mapping. It can be a predefined dictionary, dynamic registry, or automatic discovery on application startup.

Now you can serialize and deserialize types without decorating the base type with `Union` attribute

```c#
public interface IMyBaseType
{
    string? BaseProp { get; set; }
}

public abstract class MyBaseType : IMyBaseType
{
    [Key(0)]
    public string? BaseProp { get; set; }
}

[MessagePackObject]
public class DerivedClass : MyBaseType
{
    [Key(1)]
    public string? DerivedTypeProp { get; set; }
}

[MessagePackObject]
public struct DerivedStruct : IMyBaseType
{
    [Key(0)]
    public string? BaseProp { get; set; }

    [Key(1)]
    public string? DerivedStructProp { get; set; }
}

var before = new DerivedClass
{
    BaseProp = $"Base of {nameof(DerivedClass)}",
    DerivedTypeProp = nameof(DerivedClass)
};

var bin = MessagePackSerializer.Serialize<IMyBaseType>(before, options);
var debugJson = MessagePackSerializer.ConvertToJson(bin);
// [0,["Base of DerivedClass","DerivedClass"]]
Trace.WriteLine(debugJson);

var after = MessagePackSerializer.Deserialize<IMyBaseType>(bin, options);
```

## Alternative `ITypeHeaderFormatter` implementations

This repo provides an alternative implementation to encode the actual type information into the result binary data. The `TypeNameTypeHeaderFormatter` can be used without a type-to-id mapping and will encode the full type name.

## Benchmark

``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 10 (10.0.19045.2965/22H2/2022Update)
11th Gen Intel Core i7-11800H 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
```

![Benchmark results](/Images/Benchmark.png)
