namespace Prophet.Application.Interfaces.Llm;

/// <summary>Multi-LLM adapter port (MAF §10) — implementations live under <c>Prophet.Adapters.Llm</c>.</summary>
public interface ILlmAdapter
{
    Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default);
}
