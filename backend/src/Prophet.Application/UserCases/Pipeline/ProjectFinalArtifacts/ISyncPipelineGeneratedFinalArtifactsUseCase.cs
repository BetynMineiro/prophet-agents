namespace Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;

/// <summary>
/// Copies architecture, diagram, and documentation outputs from a completed pipeline version into
/// <see cref="PipelineFinalArtifact"/> rows (Markdown in Firebase), using names in
/// <see cref="PipelineGeneratedFinalArtifactNames"/>.
/// </summary>
public interface ISyncPipelineGeneratedFinalArtifactsUseCase
{
    Task ExecuteAsync(Guid projectId, Guid versionId, CancellationToken cancellationToken = default);
}
