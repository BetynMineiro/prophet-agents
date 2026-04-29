namespace Prophet.Application.Interfaces.Llm;

/// <summary>Input for a single LLM completion (MAF §10).</summary>
public sealed class LlmRequest
{
    /// <summary>E.g. <see cref="LlmCategories"/> values.</summary>
    public required string Category { get; init; }

    public required string Prompt { get; init; }

    public double Temperature { get; init; } = 0.7;

    /// <summary>Optional cap on completion tokens (provider-specific).</summary>
    public int? MaxCompletionTokens { get; init; }
}
