using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.AgentPipeline;

/// <summary>
/// Application-layer orchestration for MAF pipeline steps: invokes <see cref="IPipelineAgent"/> implementations in order and updates <see cref="ArtifactVersion"/> progress.
/// </summary>
public interface IPipelineAgentExecutor
{
    /// <summary>
    /// Executes agents in order. On success, <paramref name="version"/> has <see cref="ArtifactVersion.CurrentStepIndex"/> equal to total steps.
    /// On failure, the caller should set <see cref="ArtifactVersion.PipelineStatus"/> and <see cref="ArtifactVersion.PipelineError"/>.
    /// </summary>
    Task ExecuteAsync(Guid projectId, ArtifactVersion version, CancellationToken cancellationToken = default);

    /// <summary>Runs a single agent at <paramref name="stepIndex"/> (0-based). Updates step index to <paramref name="stepIndex"/> + 1 after success.</summary>
    Task ExecuteSingleStepAsync(Guid projectId, ArtifactVersion version, int stepIndex, CancellationToken cancellationToken = default);

    /// <summary>Runs agents from <paramref name="fromStepInclusive"/> through the last step (refinement partial re-run).</summary>
    Task ExecuteFromStepInclusiveAsync(Guid projectId, ArtifactVersion version, int fromStepInclusive, CancellationToken cancellationToken = default);
}
