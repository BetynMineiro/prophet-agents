namespace Prophet.Application.Services.EntityId;

/// <summary>
/// Generates a new entity ID. Use once per entity at creation time (9.1).
/// Implementation uses time-ordered UUID v7 for better index and ordering behavior (9.2).
/// </summary>
public interface IEntityIdGenerator
{
    /// <summary>Returns a new time-ordered Guid (UUID v7).</summary>
    Guid NewId();
}
