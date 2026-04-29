namespace Prophet.Domain.Entities.Pipeline;

/// <summary>Kind of interactive HTML proof-of-concept for a Prophet project (at most one per kind per project).</summary>
public enum HtmlPocKind
{
    /// <summary>Web / desktop browser POC.</summary>
    Web = 0,

    /// <summary>Mobile-oriented POC.</summary>
    Mobile = 1,
}
