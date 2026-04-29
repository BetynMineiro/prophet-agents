using Prophet.Application.AgentPipeline;
using Prophet.Application.Interfaces.Llm;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.AgentPipeline.Agents;

/// <summary>market-research-agent → market-analysis.json (§4 #3).</summary>
public sealed class MarketResearchAgent : IPipelineAgent
{
    public string StepId => MainPipelineStepIds.StepIds[2];

    public async Task RunAsync(PipelineAgentRunContext context, CancellationToken cancellationToken = default)
    {
        var chunks = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.Chunks, cancellationToken).ConfigureAwait(false);
        var insights = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.Insights, cancellationToken).ConfigureAwait(false);
        if (insights == null)
            throw new InvalidOperationException("Missing insights artifact.");

        var body = """
            You are the market-research-agent. Using the domain summary and structure from insights.json, respond with a single JSON object only (no markdown fences):
            {
              "competitors": [ { "name": "string", "notes": "string" } ],
              "patterns": [ "string" ],
              "risks": [ "string" ],
              "summary": "string"
            }
            insights.json:

            """ + insights.ContentJson;

        var prompt = PipelineSourceContextPrompt.PrependSourceAnchoring(body, chunks?.ContentJson);

        var response = await context.CompleteAsync(LlmCategories.Research, prompt, temperature: 0.4, maxCompletionTokens: 8192, cancellationToken).ConfigureAwait(false);
        var json = PipelineJsonFormatting.NormalizeJsonObject(response.Content);
        await context.SaveArtifactJsonAsync(ArtifactTypeNames.MarketAnalysis, "market-research-agent", json, cancellationToken).ConfigureAwait(false);
    }
}
