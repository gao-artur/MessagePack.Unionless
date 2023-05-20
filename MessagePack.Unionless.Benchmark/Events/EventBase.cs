namespace MessagePack.Unionless.Benchmark.Events;

[MessagePackObject]
[Union(0, typeof(EnrichmentFieldsEvaluatedEvent))]
[Union(1, typeof(PairingRuleSubscriptionFilterContentRecordEvent))]
[Union(2, typeof(RiskDismissedEvent))]
[Union(3, typeof(RiskSynchedEvent))]
public abstract class EventBase
{
    [Key(0)]
    public int OrgId { get; set; }
    [Key(1)]
    public string? FlowId { get; set; }
}

public static class EventsTypeMap
{
    public static IReadOnlyDictionary<Type, int> TypeToIdMap { get; } = new Dictionary<Type, int>
    {
        [typeof(EnrichmentFieldsEvaluatedEvent)] = 0,
        [typeof(PairingRuleSubscriptionFilterContentRecordEvent)] = 1,
        [typeof(RiskDismissedEvent)] = 2,
        [typeof(RiskSynchedEvent)] = 3
    };

    public static IReadOnlyDictionary<int, Type> IdToTypeMap { get; } =
        TypeToIdMap.ToDictionary(kv => kv.Value, kv => kv.Key);
}