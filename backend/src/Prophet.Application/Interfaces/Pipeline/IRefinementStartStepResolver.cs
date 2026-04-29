namespace Prophet.Application.Interfaces.Pipeline;

/// <summary>
/// Decides the first pipeline step to re-execute after a refinement request (Phase 5).
/// Lower indices preserve more parent outputs (copied into the new version).
/// </summary>
public interface IRefinementStartStepResolver
{
    /// <returns>0-based step index in <see cref="Prophet.Domain.Entities.Pipeline.MainPipelineStepIds"/> to start from (inclusive).</returns>
    Task<int> ResolveStartStepIndexAsync(string changeSummary, CancellationToken cancellationToken = default);
}
