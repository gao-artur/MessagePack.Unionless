using System.Diagnostics;
using MessagePack.Resolvers;

namespace MessagePack.Unionless.UnitTests;

[TestClass]
public class UnionlessFormatterTests
{
    [TestMethod]
    [DataRow(nameof(TypeIdTypeHeaderFormatter))]
    [DataRow(nameof(TypeNameTypeHeaderFormatter))]
    public void TestUnionHeaderFormatters_SubUnionClass(string headerFormatterName)
    {
        var headerFormatter = GetTypeHeaderFormatter(headerFormatterName);
        var options = GetOptions(headerFormatter);

        var before = new SubUnionClass
        {
            BaseProp = $"Base of {nameof(SubUnionClass)}",
            SubUnionType1Prop = nameof(SubUnionClass)
        };

        var bin = MessagePackSerializer.Serialize<IMyBaseType>(before, options);

        var debugJson = MessagePackSerializer.ConvertToJson(bin);
        Trace.WriteLine(debugJson);

        var deserialized = MessagePackSerializer.Deserialize<IMyBaseType>(bin, options);

        Assert.IsInstanceOfType<SubUnionClass>(deserialized);
        var after = (SubUnionClass)deserialized;

        Assert.AreEqual(before.BaseProp, after.BaseProp);
        Assert.AreEqual(before.SubUnionType1Prop, after.SubUnionType1Prop);
    }

    [TestMethod]
    [DataRow(nameof(TypeIdTypeHeaderFormatter))]
    [DataRow(nameof(TypeNameTypeHeaderFormatter))]
    public void TestUnionHeaderFormatters_SubUnionStruct(string headerFormatterName)
    {
        var headerFormatter = GetTypeHeaderFormatter(headerFormatterName);
        var options = GetOptions(headerFormatter);

        var before = new SubUnionStruct
        {
            BaseProp = $"Base of {nameof(SubUnionStruct)}",
            SubUnionType2Prop = nameof(SubUnionStruct)
        };

        var bin = MessagePackSerializer.Serialize<IMyBaseType>(before, options);

        var debugJson = MessagePackSerializer.ConvertToJson(bin);
        Trace.WriteLine(debugJson);

        var deserialized = MessagePackSerializer.Deserialize<IMyBaseType>(bin, options);

        Assert.IsInstanceOfType<SubUnionStruct>(deserialized);
        var after = (SubUnionStruct)deserialized;

        Assert.AreEqual(before.BaseProp, after.BaseProp);
        Assert.AreEqual(before.SubUnionType2Prop, after.SubUnionType2Prop);
    }

    private static MessagePackSerializerOptions GetOptions(ITypeHeaderFormatter headerFormatter)
    {
        var resolver = CompositeResolver.Create(
            MyBaseTypeFormatterResolver.Instance,
            StandardResolver.Instance);

        var options = MessagePackSerializer.DefaultOptions.WithResolver(resolver).WithOmitAssemblyVersion(true);

        return new UnionlessMessagePackSerializerOptions(options)
        {
            TypeHeaderFormatter = headerFormatter
        };
    }

    private ITypeHeaderFormatter GetTypeHeaderFormatter(string typeName)
    {
        var typeToIdMap = new Dictionary<Type, int>
        {
            [typeof(SubUnionClass)] = 0,
            [typeof(SubUnionStruct)] = 1
        };

        var idToTypeMap = typeToIdMap.ToDictionary(kv => kv.Value, kv => kv.Key);

        switch (typeName)
        {
            case nameof(TypeIdTypeHeaderFormatter):
                return new TypeIdTypeHeaderFormatter(typeToIdMap, idToTypeMap);
            case nameof(TypeNameTypeHeaderFormatter):
                return new TypeNameTypeHeaderFormatter();
            default:
                Assert.Fail($"Can't find formatter with name '{typeName}'");
                // satisfy compiler
                throw new Exception();
        }
    }
}