using Prophet.Application.Interfaces.Llm;

namespace Prophet.Adapters.Llm;

/// <summary>Fails fast when no LLM keys are configured — keeps DI valid in tests.</summary>
public sealed class DisabledLlmAdapter : ILlmAdapter
{
    public Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default) =>
        Task.FromException<LlmResponse>(new InvalidOperationException(
            "LLM is disabled: set ApiKey on at least one entry under Llm:Providers (User Secrets or environment)."));
}
