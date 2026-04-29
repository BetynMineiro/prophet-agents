using Prophet.Application.AgentPipeline;
using Prophet.Application.Interfaces.Llm;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.AgentPipeline.Agents;

/// <summary>model-agent → domain-model.json (§4 #4).</summary>
public sealed class ModelAgent : IPipelineAgent
{
    public string StepId => MainPipelineStepIds.StepIds[3];

    public async Task RunAsync(PipelineAgentRunContext context, CancellationToken cancellationToken = default)
    {
        var chunks = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.Chunks, cancellationToken).ConfigureAwait(false);
        var insights = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.Insights, cancellationToken).ConfigureAwait(false);
        var market = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.MarketAnalysis, cancellationToken).ConfigureAwait(false);
        if (insights == null)
            throw new InvalidOperationException("Missing insights artifact.");

        var body = """
            You are the model-agent. Build a domain model as one JSON object only (no markdown fences):
            {
              "aggregates": [ { "name": "string", "description": "string", "entities": [ "string" ] } ],
              "valueObjects": [ { "name": "string", "description": "string" } ],
              "relationships": [ { "from": "string", "to": "string", "kind": "string" } ],
              "ubiquitousLanguage": [ { "term": "string", "definition": "string" } ]
            }
            insights.json:

            """ + insights.ContentJson + """

            market-analysis.json:

            """ + (market?.ContentJson ?? "{}");

        var prompt = PipelineSourceContextPrompt.PrependSourceAnchoring(body, chunks?.ContentJson);

        var response = await context.CompleteAsync(LlmCategories.Structured, prompt, temperature: 0.2, maxCompletionTokens: 8192, cancellationToken).ConfigureAwait(false);
        var json = PipelineJsonFormatting.NormalizeJsonObject(response.Content);
        await context.SaveArtifactJsonAsync(ArtifactTypeNames.DomainModel, "model-agent", json, cancellationToken).ConfigureAwait(false);
    }
}
