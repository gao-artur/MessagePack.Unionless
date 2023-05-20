using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using MessagePack.Resolvers;
using MessagePack.Unionless.Benchmark.Events;

namespace MessagePack.Unionless.Benchmark;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class UnionSerializerBenchmark
{
    private const int N = 1;

    private static readonly MessagePackSerializerOptions UnionOptions = MessagePackSerializer.DefaultOptions;

    private static readonly MessagePackSerializerOptions TypeIdOptions = new UnionlessMessagePackSerializerOptions(
        MessagePackSerializer.DefaultOptions
            .WithResolver(CompositeResolver.Create(
                MyBaseTypeFormatterResolver.Instance,
                StandardResolver.Instance))
            .WithOmitAssemblyVersion(true))
    {
        TypeHeaderFormatter = new TypeIdTypeHeaderFormatter(EventsTypeMap.TypeToIdMap)
    };

    private static readonly MessagePackSerializerOptions TypeNameOptions = new UnionlessMessagePackSerializerOptions(
        MessagePackSerializer.DefaultOptions
            .WithResolver(CompositeResolver.Create(
                MyBaseTypeFormatterResolver.Instance,
                StandardResolver.Instance))
            .WithOmitAssemblyVersion(true))
    {
        TypeHeaderFormatter = new TypeNameTypeHeaderFormatter()
    };

    private readonly IList<byte[]> _serializedEventsUnionOptions = new List<byte[]>();
    private readonly IList<byte[]> _serializedEventsTypeIdOptions = new List<byte[]>();
    private readonly IList<byte[]> _serializedEventsTypeNameOptions = new List<byte[]>();

    private readonly List<EventBase> _events = InitEvents();

    [GlobalSetup]
    public void Setup()
    {
        foreach (var @event in _events)
        {
            var unionBin = MessagePackSerializer.Serialize(@event, UnionOptions);
            _serializedEventsUnionOptions.Add(unionBin);
            MessagePackSerializer.Deserialize<EventBase>(unionBin, UnionOptions);
#if DEBUG
            Console.WriteLine(MessagePackSerializer.ConvertToJson(unionBin));
            Console.WriteLine();
#endif

            var typeIdBin = MessagePackSerializer.Serialize(@event, TypeIdOptions);
            _serializedEventsTypeIdOptions.Add(typeIdBin);
            MessagePackSerializer.Deserialize<EventBase>(typeIdBin, TypeIdOptions);
#if DEBUG
            Console.WriteLine(MessagePackSerializer.ConvertToJson(typeIdBin));
            Console.WriteLine(); 
#endif

            var byNameBin = MessagePackSerializer.Serialize(@event, TypeNameOptions);
            _serializedEventsTypeNameOptions.Add(byNameBin);
            MessagePackSerializer.Deserialize<EventBase>(byNameBin, TypeNameOptions);
#if DEBUG
            Console.WriteLine(MessagePackSerializer.ConvertToJson(byNameBin));
            Console.WriteLine(); 
#endif
        }
        
        var unionSize = _serializedEventsUnionOptions.First().Length;
        var idSize = _serializedEventsTypeIdOptions.First().Length;
        var nameSize = _serializedEventsTypeNameOptions.First().Length;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Serialize")]
    public void SerializeUnion()
    {
        for (var i = 0; i < N; i++)
        {
            foreach (var @event in _events)
            {
                MessagePackSerializer.Serialize(@event, UnionOptions);
            }
        }
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Deserialize")]
    public void DeserializeUnion()
    {
        for (var i = 0; i < N; i++)
        {
            foreach (var serializedEvent in _serializedEventsUnionOptions)
            {
                MessagePackSerializer.Deserialize<EventBase>(serializedEvent, UnionOptions);
            }
        }
    }
    
    [Benchmark]
    [BenchmarkCategory("Serialize")]
    public void SerializeTypeId()
    {
        for (var i = 0; i < N; i++)
        {
            foreach (var @event in _events)
            {
                MessagePackSerializer.Serialize(@event, TypeIdOptions);
            }
        }
    }
    
    [Benchmark]
    [BenchmarkCategory("Deserialize")]
    public void DeserializeTypeId()
    {
        for (var i = 0; i < N; i++)
        {
            foreach (var serializedEvent in _serializedEventsTypeIdOptions)
            {
                MessagePackSerializer.Deserialize<EventBase>(serializedEvent, TypeIdOptions);
            }
        }
    }

    [Benchmark]
    [BenchmarkCategory("Serialize")]
    public void SerializeTypeName()
    {
        for (var i = 0; i < N; i++)
        {
            foreach (var @event in _events)
            {
                MessagePackSerializer.Serialize(@event, TypeNameOptions);
            }
        }
    }

    [Benchmark]
    [BenchmarkCategory("Deserialize")]
    public void DeserializeTypeName()
    {
        for (var i = 0; i < N; i++)
        {
            foreach (var serializedEvent in _serializedEventsTypeNameOptions)
            {
                MessagePackSerializer.Deserialize<EventBase>(serializedEvent, TypeNameOptions);
            }
        }
    }

    public static List<EventBase> InitEvents()
    {
        return new List<EventBase>
        {
            new RiskSynchedEvent
            {
                OrgId = 15,
                FlowId = "[15.sync-service-dfbd59bf7-8v4qs]__403e64d094f44991bb3a03c3081f6029",
                RiskId = "bcca98a0-873e-4339-abe8-a1659e2304de",
                RootId = "",
                ServiceId = "19441",
                ContentTypeId = 1,
                ContentRecordId = "000000856493785",
                EventEntityType = 0,
                LatencyTimeMilli = 11350,
                TriggeringEventsIds = new List<string>
                {
                    "[2023-03-31T20:59:07.1722431Z][RiskDismissed][4aa0b1e3-e6f5-4883-81f7-aa21c649dd7b]"
                }
            },
            new PairingRuleSubscriptionFilterContentRecordEvent
            {
                OrgId = 15,
                FlowId = "d8679204-f758-4f05-b7ba-c3c93eda8579",
                GroupId = 9,
                ServiceId = "19693",
                PassedFilter = false,
                ContentTypeId = 1,
                SubscriptionId = 169,
                ContentRecordId = "000000856493785",
                EventEntityType = 0
            },
            new RiskDismissedEvent
            {
                OrgId = 15,
                FlowId = "5a5566e4-1b92-49b1-a042-1922e44ddb55",
                RiskId = "bcca98a0-873e-4339-abe8-a1659e2304de",
                JourneyId = 396,
                ServiceId = "19474",
                TatInHours = 2.2063f,
                ContentRecordId = null,
                RiskSeverity = 3,
                CausingReason = "InternallyUpdated",
                ContentTypeId = 1,
                EventEntityType = 1,
                LatencyTimeMilli = 1030,
                RiskProfileItemId = 4391
            },
            new EnrichmentFieldsEvaluatedEvent
            {
                OrgId = 15,
                FlowId = "5a5566e4-1b92-49b1-a042-1922e44ddb55",
                ServiceId = "19474",
                ContentTypeId = 1,
                ContentRecordId = "000000856493785",
                EventEntityType = 0,
                EvaluationPassed = false,
                RiskProfileItemIds = new[] { 3717 }
            }
        };
    }
}