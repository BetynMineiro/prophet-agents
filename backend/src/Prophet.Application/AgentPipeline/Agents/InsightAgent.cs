using Prophet.Application.Interfaces.Llm;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.AgentPipeline.Agents;

/// <summary>insight-agent → insights.json (§4 #2).</summary>
public sealed class InsightAgent : IPipelineAgent
{
    public string StepId => MainPipelineStepIds.StepIds[1];

    public async Task RunAsync(PipelineAgentRunContext context, CancellationToken cancellationToken = default)
    {
        var chunks = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.Chunks, cancellationToken).ConfigureAwait(false);
        if (chunks == null)
            throw new InvalidOperationException("Missing chunks artifact; file-agent must run first.");

        var prompt = """
            You are the insight-agent for a domain modeling pipeline.
            Read the following chunks.json (UTF-8). Respond with a single JSON object only (no markdown fences), using this shape:
            {
              "entities": [ { "name": "string", "description": "string" } ],
              "useCases": [ { "title": "string", "summary": "string" } ],
              "rules": [ "string" ],
              "domainSummary": "short paragraph for downstream agents"
            }
            chunks.json:

            """ + chunks.ContentJson;

        var response = await context.CompleteAsync(LlmCategories.Reasoning, prompt, temperature: 0.2, maxCompletionTokens: 8192, cancellationToken).ConfigureAwait(false);
        var json = PipelineJsonFormatting.NormalizeJsonObject(response.Content);
        await context.SaveArtifactJsonAsync(ArtifactTypeNames.Insights, "insight-agent", json, cancellationToken).ConfigureAwait(false);
    }
}
