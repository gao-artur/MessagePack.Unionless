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

        var before = new DerivedClass
        {
            BaseProp = $"Base of {nameof(DerivedClass)}",
            DerivedTypeProp = nameof(DerivedClass)
        };

        var bin = MessagePackSerializer.Serialize<IMyBaseType>(before, options);

        var debugJson = MessagePackSerializer.ConvertToJson(bin);
        Trace.WriteLine(debugJson);

        var deserialized = MessagePackSerializer.Deserialize<IMyBaseType>(bin, options);

        Assert.IsInstanceOfType<DerivedClass>(deserialized);
        var after = (DerivedClass)deserialized;

        Assert.AreEqual(before.BaseProp, after.BaseProp);
        Assert.AreEqual(before.DerivedTypeProp, after.DerivedTypeProp);
    }

    [TestMethod]
    [DataRow(nameof(TypeIdTypeHeaderFormatter))]
    [DataRow(nameof(TypeNameTypeHeaderFormatter))]
    public void TestUnionHeaderFormatters_SubUnionStruct(string headerFormatterName)
    {
        var headerFormatter = GetTypeHeaderFormatter(headerFormatterName);
        var options = GetOptions(headerFormatter);

        var before = new DerivedStruct
        {
            BaseProp = $"Base of {nameof(DerivedStruct)}",
            DerivedStructProp = nameof(DerivedStruct)
        };

        var bin = MessagePackSerializer.Serialize<IMyBaseType>(before, options);

        var debugJson = MessagePackSerializer.ConvertToJson(bin);
        Trace.WriteLine(debugJson);

        var deserialized = MessagePackSerializer.Deserialize<IMyBaseType>(bin, options);

        Assert.IsInstanceOfType<DerivedStruct>(deserialized);
        var after = (DerivedStruct)deserialized;

        Assert.AreEqual(before.BaseProp, after.BaseProp);
        Assert.AreEqual(before.DerivedStructProp, after.DerivedStructProp);
    }

    [TestMethod]
    [DataRow(nameof(TypeIdTypeHeaderFormatter))]
    [DataRow(nameof(TypeNameTypeHeaderFormatter))]
    public void TestUnionHeaderFormatters_Collection(string headerFormatterName)
    {
        var headerFormatter = GetTypeHeaderFormatter(headerFormatterName);
        var options = GetOptions(headerFormatter);

        var firstBefore = new DerivedClass
        {
            BaseProp = $"Base of {nameof(DerivedClass)}",
            DerivedTypeProp = nameof(DerivedClass)
        };

        var secondBefore = new DerivedStruct
        {
            BaseProp = $"Base of {nameof(DerivedStruct)}",
            DerivedStructProp = nameof(DerivedStruct)
        };

        var before = new IMyBaseType[]
        {
            firstBefore,
            secondBefore
        };

        var bin = MessagePackSerializer.Serialize(before, options);

        var debugJson = MessagePackSerializer.ConvertToJson(bin);
        Trace.WriteLine(debugJson);

        var deserialized = MessagePackSerializer.Deserialize<IMyBaseType[]>(bin, options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(2, deserialized.Length);

        var first = deserialized[0];
        Assert.IsInstanceOfType<DerivedClass>(first);
        var firstAfter = (DerivedClass)first;

        Assert.AreEqual(firstBefore.BaseProp, firstAfter.BaseProp);
        Assert.AreEqual(firstBefore.DerivedTypeProp, firstAfter.DerivedTypeProp);

        var second = deserialized[1];
        Assert.IsInstanceOfType<DerivedStruct>(second);
        var secondAfter = (DerivedStruct)second;

        Assert.AreEqual(secondBefore.BaseProp, secondAfter.BaseProp);
        Assert.AreEqual(secondBefore.DerivedStructProp, secondAfter.DerivedStructProp);
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
            [typeof(DerivedClass)] = 0,
            [typeof(DerivedStruct)] = 1
        };

        switch (typeName)
        {
            case nameof(TypeIdTypeHeaderFormatter):
                return new TypeIdTypeHeaderFormatter(typeToIdMap);
            case nameof(TypeNameTypeHeaderFormatter):
                return new TypeNameTypeHeaderFormatter();
            default:
                Assert.Fail($"Can't find formatter with name '{typeName}'");
                // satisfy compiler
                throw new Exception();
        }
    }
}