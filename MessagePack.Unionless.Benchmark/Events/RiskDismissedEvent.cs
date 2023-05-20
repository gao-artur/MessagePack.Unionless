namespace MessagePack.Unionless.Benchmark.Events;

[MessagePackObject]
public class RiskDismissedEvent : EventBase
{
    [Key(2)]
    public string? RiskId { get; set; }
    [Key(3)]
    public int JourneyId { get; set; }
    [Key(4)]
    public string? ServiceId { get; set; }
    [Key(5)]
    public float TatInHours { get; set; }
    [Key(6)]
    public int RiskSeverity { get; set; }
    [Key(7)]
    public string? CausingReason { get; set; }
    [Key(8)]
    public int ContentTypeId { get; set; }
    [Key(9)]
    public string? ContentRecordId { get; set; }
    [Key(10)]
    public int EventEntityType { get; set; }
    [Key(11)]
    public int LatencyTimeMilli { get; set; }
    [Key(12)]
    public int RiskProfileItemId { get; set; }
}