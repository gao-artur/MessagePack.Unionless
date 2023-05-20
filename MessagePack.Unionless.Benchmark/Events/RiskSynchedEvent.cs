namespace MessagePack.Unionless.Benchmark.Events;

[MessagePackObject]
public class RiskSynchedEvent : EventBase
{
    [Key(2)]
    public string? RiskId { get; set; }
    [Key(3)]
    public string? RootId { get; set; }
    [Key(4)]
    public string? ServiceId { get; set; }
    [Key(5)]
    public int ContentTypeId { get; set; }
    [Key(6)]
    public string? ContentRecordId { get; set; }
    [Key(7)]
    public int EventEntityType { get; set; }
    [Key(8)]
    public int LatencyTimeMilli { get; set; }
    [Key(9)]
    public List<string>? TriggeringEventsIds { get; set; }
}