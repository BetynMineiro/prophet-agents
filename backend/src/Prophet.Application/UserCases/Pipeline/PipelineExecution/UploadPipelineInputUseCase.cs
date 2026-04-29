using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Services.EntityId;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

public sealed class UploadPipelineInputUseCase(
    IPipelineExecutionStore pipelineStore,
    IUploadPipelineVersionFileUseCase uploadVersionFile,
    IEntityIdGenerator idGenerator) : IUploadPipelineInputUseCase
{
    public const long MaxFileBytes = PipelineVersionUploadLimits.MaxFileBytes;

    public async Task<PipelineVersionFileItemDto?> ExecuteAsync(
        Guid projectId,
        Guid? versionId,
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (!await pipelineStore.ProjectExistsActiveAsync(projectId, cancellationToken).ConfigureAwait(false))
            return null;

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (buffer.Length > MaxFileBytes)
            return null;

        ArtifactVersion version;
        if (versionId is { } vid)
        {
            var existing = await pipelineStore.GetVersionForProjectAsync(projectId, vid, cancellationToken).ConfigureAwait(false);
            if (existing == null)
                return null;
            version = existing;
        }
        else
        {
            var userId = Guid.Empty;
            var next = await pipelineStore.GetMaxVersionNumberAsync(projectId, cancellationToken).ConfigureAwait(false) + 1;
            version = new ArtifactVersion
            {
                Id = idGenerator.NewId(),
                PipelineProjectId = projectId,
                VersionNumber = next,
                ParentVersionId = null,
                ChangeSummary = null,
                PipelineStatus = PipelineRunStatus.Idle,
                CurrentStepIndex = 0,
            };
            version.SetCreatedBy(userId);
            version = await pipelineStore.AddVersionAsync(version, cancellationToken).ConfigureAwait(false);
        }

        buffer.Position = 0;
        var safeName = SanitizeFileName(originalFileName);
        var ct = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType.Trim();

        return await uploadVersionFile.ExecuteAsync(
            projectId,
            version.Id,
            version.VersionNumber,
            "input",
            ArtifactFileTypeNames.Input,
            buffer,
            safeName,
            ct,
            cancellationToken).ConfigureAwait(false);
    }

    private static string SanitizeFileName(string name)
    {
        var s = string.IsNullOrWhiteSpace(name) ? "upload.bin" : Path.GetFileName(name.Trim());
        return s.Length > 512 ? s[..512] : s;
    }
}
