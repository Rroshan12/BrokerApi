namespace HouseBroker.Domain.Common;

/// <summary>
/// DOMAIN LAYER: the core of the application. It holds business entities and rules
/// and has no dependencies on any other project, framework or database.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
