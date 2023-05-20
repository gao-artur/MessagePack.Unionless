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

|              Method |  Categories |       Mean |    Error |   StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|-------------------- |------------ |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
|    DeserializeUnion | Deserialize | 1,228.5 ns | 10.49 ns |  9.82 ns |  1.00 |    0.00 | 0.1278 |    1616 B |        1.00 |
|   DeserializeTypeId | Deserialize | 1,621.6 ns |  5.00 ns |  4.17 ns |  1.32 |    0.01 | 0.1278 |    1616 B |        1.00 |
| DeserializeTypeName | Deserialize | 1,958.5 ns | 18.25 ns | 16.18 ns |  1.60 |    0.02 | 0.1259 |    1616 B |        1.00 |
|                     |             |            |          |          |       |         |        |           |             |
|      SerializeUnion |   Serialize |   914.8 ns |  3.69 ns |  3.08 ns |  1.00 |    0.00 | 0.0477 |     600 B |        1.00 |
|     SerializeTypeId |   Serialize | 1,140.2 ns |  3.16 ns |  2.80 ns |  1.25 |    0.00 | 0.0477 |     600 B |        1.00 |
|   SerializeTypeName |   Serialize | 1,121.1 ns |  7.38 ns |  6.16 ns |  1.23 |    0.01 | 0.0687 |     872 B |        1.45 |
