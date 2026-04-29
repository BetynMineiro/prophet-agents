using System.Text.Json;
using Prophet.Application.AgentPipeline;
using Prophet.Application.Interfaces.Llm;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.AgentPipeline.Agents;

/// <summary>diagram-agent → class-diagram + flow-diagram JSON (§4 #6).</summary>
public sealed class DiagramAgent : IPipelineAgent
{
    public string StepId => MainPipelineStepIds.StepIds[5];

    public async Task RunAsync(PipelineAgentRunContext context, CancellationToken cancellationToken = default)
    {
        var chunks = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.Chunks, cancellationToken).ConfigureAwait(false);
        var domain = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.DomainModel, cancellationToken).ConfigureAwait(false);
        if (domain == null)
            throw new InvalidOperationException("Missing domain-model artifact.");

        var body = """
            You are the diagram-agent. Output a single JSON object only (no markdown fences) with Mermaid source strings:
            {
              "classDiagram": { "mermaid": "classDiagram\n..." },
              "flowDiagram": { "mermaid": "flowchart TD\n..." }
            }

            Strict syntax rules (invalid Mermaid will break rendering):
            - classDiagram: use simple identifiers for class names (no spaces). Relations: `A --> B` or `A --> B : shortLabel` where shortLabel is a single word or use quotes only where Mermaid docs allow. Do NOT put quoted node names immediately before ` : label` in a way that duplicates arrows on the same line.
            - flowchart TD: edges are `A --> B` or `A -->|edge text| B` (pipe form). Do NOT use sequence-diagram style `A --> B : label` on flowcharts — that is invalid. Do NOT chain `--> "X" : Y -->` on one line.
            - One statement per line; avoid ambiguous `:` after `-->` unless using classDiagram relation form exactly as in Mermaid class docs.
            - Keep diagrams small enough to parse (under ~60 nodes/edges each).
            Prefer names and flows reflected in domain-model.json and the original source; do not introduce entities solely from generic industry knowledge.

            Base diagrams on:

            """ + domain.ContentJson;

        var prompt = PipelineSourceContextPrompt.PrependSourceAnchoring(body, chunks?.ContentJson, PipelineSourceContextPrompt.DiagramAndPocMaxChars);

        var response = await context.CompleteAsync(LlmCategories.Structured, prompt, temperature: 0.1, maxCompletionTokens: 8192, cancellationToken).ConfigureAwait(false);
        var normalized = PipelineJsonFormatting.NormalizeJsonObject(response.Content);

        using var doc = JsonDocument.Parse(normalized);
        var root = doc.RootElement;
        var classJson = root.TryGetProperty("classDiagram", out var c)
            ? c.GetRawText()
            : JsonSerializer.Serialize(new { mermaid = "classDiagram\nclass Placeholder" }, PipelineJsonDefaults.SerializerOptions);
        var flowJson = root.TryGetProperty("flowDiagram", out var f)
            ? f.GetRawText()
            : JsonSerializer.Serialize(new { mermaid = "flowchart TD\nA[Start] --> B[End]" }, PipelineJsonDefaults.SerializerOptions);

        await context.SaveArtifactJsonAsync(ArtifactTypeNames.ClassDiagram, "diagram-agent", classJson, cancellationToken).ConfigureAwait(false);
        await context.SaveArtifactJsonAsync(ArtifactTypeNames.FlowDiagram, "diagram-agent", flowJson, cancellationToken).ConfigureAwait(false);
    }
}
