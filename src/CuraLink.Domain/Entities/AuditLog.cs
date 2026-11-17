namespace CuraLink.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public string? ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public string? CorrelationId { get; set; }
    public string? MetadataJson { get; set; }
}
