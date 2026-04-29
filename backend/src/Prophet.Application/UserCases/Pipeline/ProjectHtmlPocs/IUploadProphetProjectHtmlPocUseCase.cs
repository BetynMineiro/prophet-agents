using Prophet.Application.UserCases.Pipeline.ProjectInputs;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;

public interface IUploadPipelineHtmlPocUseCase
{
    Task<UploadPipelineHtmlPocExecutionResult?> ExecuteAsync(
        Guid projectId,
        HtmlPocKind kind,
        InputFileChunk file,
        bool replaceConfirmed,
        CancellationToken cancellationToken = default);
}
