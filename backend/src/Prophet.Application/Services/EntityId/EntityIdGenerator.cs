namespace Prophet.Application.Services.EntityId;

/// <summary>
/// Generates time-ordered UUID v7 (9.2). Good for indexes and chronological ordering.
/// </summary>
public sealed class EntityIdGenerator : IEntityIdGenerator
{
    public Guid NewId() => Guid.CreateVersion7();
}
