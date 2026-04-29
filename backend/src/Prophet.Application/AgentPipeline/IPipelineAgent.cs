namespace Prophet.Application.AgentPipeline;

/// <summary>
/// One step in the MAF pipeline (Application layer): builds prompts, calls the LLM via <see cref="PipelineAgentRunContext"/>,
/// persists artifacts. No direct HTTP/SDK to model providers — those stay in the LLM adapter.
/// </summary>
public interface IPipelineAgent
{
    /// <summary>Matches <see cref="Prophet.Domain.Entities.Pipeline.MainPipelineStepIds"/> entry for this step.</summary>
    string StepId { get; }

    /// <summary>Loads inputs from <paramref name="context"/>, completes the LLM call, writes outputs.</summary>
    Task RunAsync(PipelineAgentRunContext context, CancellationToken cancellationToken = default);
}
