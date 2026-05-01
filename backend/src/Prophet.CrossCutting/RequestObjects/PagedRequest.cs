namespace Prophet.CrossCutting.RequestObjects;

public enum ActiveState
{
    All = 0,
    Active = 1,
    Inactive = 2
}

/// <summary>
/// Pagination request. Supports offset (PageNumber/PageSize) and cursor (Cursor + PageSize as limit).
/// </summary>
public class PagedRequest
{
    /// <summary>Items per page / limit (also used as limit in cursor-based pagination).</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>Optional cursor for cursor-based pagination (e.g. last returned ID).</summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// Optional text filter applied server-side to string fields only.
    /// When provided, it should be trimmed and truncated (max 200 chars).
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Filter by entity status.
    /// - All: includes both active + inactive
    /// - Active: only active
    /// - Inactive: only inactive
    /// </summary>
    public ActiveState ActiveState { get; set; } = ActiveState.All;
}
