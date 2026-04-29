namespace Prophet.Application.Interfaces.Llm;

/// <summary>Normalized result from any LLM provider.</summary>
public sealed class LlmResponse
{
    public required string Content { get; init; }

    /// <summary>Provider-reported model id, when available.</summary>
    public string? Model { get; init; }
}
