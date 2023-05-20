namespace MessagePack.Unionless.Benchmark.Events;

[MessagePackObject]
public class PairingRuleSubscriptionFilterContentRecordEvent : EventBase
{
    [Key(2)]
    public int GroupId { get; set; }
    [Key(3)]
    public string? ServiceId { get; set; }
    [Key(4)]
    public bool PassedFilter { get; set; }
    [Key(5)]
    public int ContentTypeId { get; set; }
    [Key(6)]
    public int SubscriptionId { get; set; }
    [Key(7)]
    public string? ContentRecordId { get; set; }
    [Key(8)]
    public int EventEntityType { get; set; }
}