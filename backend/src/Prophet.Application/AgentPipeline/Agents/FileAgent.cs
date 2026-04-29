using System.Text;
using System.Text.Json;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.AgentPipeline.Agents;

/// <summary>file-agent → chunks.json (§4 #1).</summary>
public sealed class FileAgent(IPipelineInputDocumentStore inputDocumentStore) : IPipelineAgent
{
    public string StepId => MainPipelineStepIds.StepIds[0];

    public async Task RunAsync(PipelineAgentRunContext context, CancellationToken cancellationToken = default)
    {
        var files = await context.Store.ListFilesAsync(context.VersionId, cancellationToken).ConfigureAwait(false);
        var versionInputs = files
            .Where(f => f.FileType == ArtifactFileTypeNames.Input)
            .OrderBy(f => f.CreatedAtUtc)
            .ToList();

        var projectDocs = await inputDocumentStore.ListByProjectAsync(context.ProjectId, cancellationToken).ConfigureAwait(false);
        var orderedProject = projectDocs.OrderBy(d => d.CreatedAtUtc).ToList();

        if (orderedProject.Count == 0 && versionInputs.Count == 0)
            throw new InvalidOperationException(
                "No input files for this version and no project inputs. Upload documents under project inputs or version upload first.");

        var totalDocs = orderedProject.Count + versionInputs.Count;
        var needHeaders = totalDocs > 1;

        var sb = new StringBuilder();
        var sourceNames = new List<string>();

        foreach (var doc in orderedProject)
        {
            var bytes = await context.Storage.ReadObjectAsync(doc.StorageObjectPath, cancellationToken).ConfigureAwait(false);
            if (bytes == null || bytes.Length == 0)
                throw new InvalidOperationException($"Input file could not be read from storage: {doc.OriginalFileName}.");

            var text = PipelineInputBinaryTextExtractor.Extract(bytes, doc.OriginalFileName);
            if (needHeaders)
            {
                sb.AppendLine($"--- {doc.OriginalFileName} ---");
                sb.AppendLine(text);
                sb.AppendLine();
            }
            else
                sb.Append(text);

            sourceNames.Add(doc.OriginalFileName);
        }

        foreach (var input in versionInputs)
        {
            var label = input.OriginalFileName ?? "upload";
            var bytes = await context.Storage.ReadObjectAsync(input.StorageObjectPath, cancellationToken).ConfigureAwait(false);
            if (bytes == null || bytes.Length == 0)
                throw new InvalidOperationException($"Input file could not be read from storage: {input.OriginalFileName ?? input.Id.ToString()}.");

            var text = PipelineInputBinaryTextExtractor.Extract(bytes, label);
            if (needHeaders)
            {
                sb.AppendLine($"--- {label} ---");
                sb.AppendLine(text);
                sb.AppendLine();
            }
            else
                sb.Append(text);

            sourceNames.Add(label);
        }

        var combinedText = sb.ToString();

        var chunks = ChunkText(combinedText, maxChars: 12_000);
        var joinedNames = string.Join(", ", sourceNames);

        var payload = new
        {
            sourceFileName = joinedNames,
            sourceFileNames = sourceNames,
            chunks = chunks.Select((t, i) => new { index = i, language = "text", text = t }).ToList(),
        };

        var json = JsonSerializer.Serialize(payload, PipelineJsonDefaults.SerializerOptions);
        await context.SaveArtifactJsonAsync(ArtifactTypeNames.Chunks, "file-agent", json, cancellationToken).ConfigureAwait(false);
    }

    private static IReadOnlyList<string> ChunkText(string text, int maxChars)
    {
        if (string.IsNullOrEmpty(text))
            return [""];

        var list = new List<string>();
        for (var i = 0; i < text.Length; i += maxChars)
        {
            var len = Math.Min(maxChars, text.Length - i);
            list.Add(text.Substring(i, len));
        }

        return list;
    }
}
