using Prophet.Application.Interfaces.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class SetPipelineRunStepUseCase(IPipelineExecutionStore pipelineStore) : ISetPipelineRunStepUseCase
{
    public async Task ExecuteAsync(Guid projectId, Guid versionId, int stepIndex, CancellationToken cancellationToken = default)
    {
        var v = await pipelineStore.GetVersionForUpdateAsync(projectId, versionId, cancellationToken).ConfigureAwait(false);
        if (v == null)
            throw new InvalidOperationException("Artifact version not found for pipeline step update.");

        v.CurrentStepIndex = stepIndex;
        await pipelineStore.PersistChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
