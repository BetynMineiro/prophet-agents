namespace Prophet.Domain.Entities.Common;

/// <summary>
/// Base for entities with a Guid primary key and audit timestamps + who created/updated (userId from session).
/// Use for domain entities that need consistent Id + CreatedAt/UpdatedAt + CreatedBy/UpdatedBy.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    /// <summary>User (from session) who created the entity.</summary>
    public Guid? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    /// <summary>User (from session) who last updated the entity.</summary>
    public Guid? UpdatedByUserId { get; set; }

    /// <summary>Sets CreatedAtUtc and CreatedByUserId. Call when creating the entity (userId from current session).</summary>
    public void SetCreatedBy(Guid userId)
    {
        CreatedAtUtc = DateTime.UtcNow;
        CreatedByUserId = userId;
    }

    /// <summary>Sets UpdatedAtUtc and UpdatedByUserId. Call when updating the entity (userId from current session).</summary>
    public void SetUpdatedBy(Guid userId)
    {
        UpdatedAtUtc = DateTime.UtcNow;
        UpdatedByUserId = userId;
    }
}
