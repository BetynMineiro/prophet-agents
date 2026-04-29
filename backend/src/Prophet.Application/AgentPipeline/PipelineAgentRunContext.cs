using Prophet.Application.Interfaces.Llm;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;
using Prophet.Application.UserCases.Pipeline.PipelineExecution;

namespace Prophet.Application.AgentPipeline;

/// <summary>Per-run context: reads via store; writes via pipeline use cases.</summary>
public sealed class PipelineAgentRunContext(
    Guid projectId,
    Guid versionId,
    int versionNumber,
    IPipelineExecutionStore store,
    ILlmAdapter llm,
    IStorageService storage,
    IUploadPipelineVersionFileUseCase uploadVersionFile,
    IAddPipelineArtifactUseCase addPipelineArtifact)
{
    public Guid ProjectId { get; } = projectId;
    public Guid VersionId { get; } = versionId;
    public int VersionNumber { get; } = versionNumber;
    public IPipelineExecutionStore Store { get; } = store;
    public ILlmAdapter Llm { get; } = llm;

    /// <summary>Server-side read of input blobs (e.g. file-agent).</summary>
    public IStorageService Storage { get; } = storage;

    public Task<LlmResponse> CompleteAsync(
        string category,
        string prompt,
        double temperature,
        int? maxCompletionTokens,
        CancellationToken cancellationToken) =>
        Llm.CompleteAsync(
            new LlmRequest
            {
                Category = category,
                Prompt = prompt,
                Temperature = temperature,
                MaxCompletionTokens = maxCompletionTokens,
            },
            cancellationToken);

    public Task SaveArtifactJsonAsync(string artifactType, string createdByAgent, string contentJson, CancellationToken cancellationToken) =>
        addPipelineArtifact.ExecuteAsync(VersionId, artifactType, contentJson, createdByAgent, cancellationToken);

    /// <summary>Uploads under <c>v{n}/{folderSegment}/</c> via <see cref="IUploadPipelineVersionFileUseCase"/>.</summary>
    public Task<PipelineVersionFileItemDto?> UploadVersionFileAsync(
        string storageFolderSegment,
        string fileType,
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken) =>
        uploadVersionFile.ExecuteAsync(
            ProjectId,
            VersionId,
            VersionNumber,
            storageFolderSegment,
            fileType,
            content,
            originalFileName,
            contentType,
            cancellationToken);
}
