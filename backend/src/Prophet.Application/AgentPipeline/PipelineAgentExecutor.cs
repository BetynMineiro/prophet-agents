using Prophet.Application.AgentPipeline.Agents;
using Prophet.Application.Interfaces.Llm;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.Application.UserCases.Pipeline.PipelineExecution;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.AgentPipeline;

/// <summary>
/// Application-layer orchestrator: runs <see cref="IPipelineAgent"/> steps in order.
/// LLM calls go through <see cref="PipelineAgentRunContext"/> → <see cref="ILlmAdapter"/> (adapter); agents do not reference HTTP or provider SDKs.
/// </summary>
public sealed class PipelineAgentExecutor(
    IPipelineExecutionStore pipelineStore,
    ILlmAdapter llmAdapter,
    IStorageService storage,
    IUploadPipelineVersionFileUseCase uploadPipelineVersionFile,
    IAddPipelineArtifactUseCase addPipelineArtifact,
    ISetPipelineRunStepUseCase setPipelineRunStep,
    FileAgent fileAgent,
    InsightAgent insightAgent,
    MarketResearchAgent marketResearchAgent,
    ModelAgent modelAgent,
    ArchitectureAgent architectureAgent,
    DiagramAgent diagramAgent,
    PocWebAgent pocWebAgent,
    PocMobileAgent pocMobileAgent,
    DocAgent docAgent,
    PackagingAgent packagingAgent) : IPipelineAgentExecutor
{
    private readonly IPipelineAgent[] _orderedAgents =
    [
        fileAgent,
        insightAgent,
        marketResearchAgent,
        modelAgent,
        architectureAgent,
        diagramAgent,
        pocWebAgent,
        pocMobileAgent,
        docAgent,
        packagingAgent,
    ];

    public async Task ExecuteAsync(Guid projectId, ArtifactVersion version, CancellationToken cancellationToken = default)
    {
        var ctx = CreateRunContext(projectId, version.Id, version.VersionNumber);

        for (var i = 0; i < _orderedAgents.Length; i++)
        {
            await setPipelineRunStep.ExecuteAsync(projectId, version.Id, i, cancellationToken).ConfigureAwait(false);

            var agent = _orderedAgents[i];
            EnsureAgentStepMatchesCatalog(agent, i);

            await agent.RunAsync(ctx, cancellationToken).ConfigureAwait(false);

            await setPipelineRunStep.ExecuteAsync(projectId, version.Id, i + 1, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task ExecuteSingleStepAsync(Guid projectId, ArtifactVersion version, int stepIndex, CancellationToken cancellationToken = default)
    {
        if (stepIndex < 0 || stepIndex >= _orderedAgents.Length)
            throw new ArgumentOutOfRangeException(nameof(stepIndex));

        var ctx = CreateRunContext(projectId, version.Id, version.VersionNumber);

        await setPipelineRunStep.ExecuteAsync(projectId, version.Id, stepIndex, cancellationToken).ConfigureAwait(false);

        var agent = _orderedAgents[stepIndex];
        EnsureAgentStepMatchesCatalog(agent, stepIndex);

        await agent.RunAsync(ctx, cancellationToken).ConfigureAwait(false);

        await setPipelineRunStep.ExecuteAsync(projectId, version.Id, stepIndex + 1, cancellationToken).ConfigureAwait(false);
    }

    public async Task ExecuteFromStepInclusiveAsync(
        Guid projectId,
        ArtifactVersion version,
        int fromStepInclusive,
        CancellationToken cancellationToken = default)
    {
        if (fromStepInclusive < 0 || fromStepInclusive >= _orderedAgents.Length)
            throw new ArgumentOutOfRangeException(nameof(fromStepInclusive));

        var ctx = CreateRunContext(projectId, version.Id, version.VersionNumber);

        for (var i = fromStepInclusive; i < _orderedAgents.Length; i++)
        {
            await setPipelineRunStep.ExecuteAsync(projectId, version.Id, i, cancellationToken).ConfigureAwait(false);

            var agent = _orderedAgents[i];
            EnsureAgentStepMatchesCatalog(agent, i);

            await agent.RunAsync(ctx, cancellationToken).ConfigureAwait(false);

            await setPipelineRunStep.ExecuteAsync(projectId, version.Id, i + 1, cancellationToken).ConfigureAwait(false);
        }
    }

    private PipelineAgentRunContext CreateRunContext(Guid projectId, Guid versionId, int versionNumber) =>
        new(
            projectId,
            versionId,
            versionNumber,
            pipelineStore,
            llmAdapter,
            storage,
            uploadPipelineVersionFile,
            addPipelineArtifact);

    private static void EnsureAgentStepMatchesCatalog(IPipelineAgent agent, int index)
    {
        if (!string.Equals(agent.StepId, MainPipelineStepIds.StepIds[index], StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"Agent order mismatch at index {index}: expected {MainPipelineStepIds.StepIds[index]}, got {agent.StepId}.");
    }
}
