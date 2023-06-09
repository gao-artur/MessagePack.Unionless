﻿using MessagePack.Formatters;

namespace MessagePack.Unionless.UnitTests;

public sealed class MyBaseTypeFormatterResolver : IFormatterResolver
{
    private static readonly UnionlessFormatter<IMyBaseType> Formatter = new();

    private MyBaseTypeFormatterResolver()
    {
    }

    public static IFormatterResolver Instance { get; } = new MyBaseTypeFormatterResolver();

    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        return typeof(T) == typeof(IMyBaseType) ? (IMessagePackFormatter<T>)Formatter : null;
    }
}

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