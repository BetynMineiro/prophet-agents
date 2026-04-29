namespace Prophet.Application.Interfaces.Llm;

/// <summary>Categories from MAF §10 — used by <see cref="ILLMAdapter"/> routing.</summary>
public static class LlmCategories
{
    public const string Reasoning = "reasoning";
    public const string Structured = "structured";
    public const string Research = "research";

    public static bool IsKnown(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return false;
        return category.Trim().ToLowerInvariant() switch
        {
            Reasoning or Structured or Research => true,
            _ => false,
        };
    }

    /// <summary>Returns <see cref="Reasoning"/>, <see cref="Structured"/>, or <see cref="Research"/>.</summary>
    public static string Normalize(string category) => category.Trim().ToLowerInvariant();
}
