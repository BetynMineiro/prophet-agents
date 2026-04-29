using Prophet.Application.AgentPipeline;
using Prophet.Application.Interfaces.Llm;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.AgentPipeline.Agents;

/// <summary>architecture-agent → architecture.json (§4 #5).</summary>
public sealed class ArchitectureAgent : IPipelineAgent
{
    public string StepId => MainPipelineStepIds.StepIds[4];

    public async Task RunAsync(PipelineAgentRunContext context, CancellationToken cancellationToken = default)
    {
        var chunks = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.Chunks, cancellationToken).ConfigureAwait(false);
        var domain = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.DomainModel, cancellationToken).ConfigureAwait(false);
        if (domain == null)
            throw new InvalidOperationException("Missing domain-model artifact.");

        var body = """
            You are the architecture-agent. Propose a concise solution architecture as one JSON object only (no markdown fences):
            {
              "style": "string (e.g. modular monolith, microservices)",
              "layers": [ { "name": "string", "responsibility": "string" } ],
              "components": [ { "name": "string", "role": "string", "interfaces": [ "string" ] } ],
              "integrationPoints": [ "string" ],
              "notes": "string"
            }
            Ground integration points and constraints in the original source material when present; do not invent vendors or integrations absent from the source unless clearly justified as assumptions in "notes".
            domain-model.json:

            """ + domain.ContentJson;

        var prompt = PipelineSourceContextPrompt.PrependSourceAnchoring(body, chunks?.ContentJson, PipelineSourceContextPrompt.ArchitectureMaxChars);

        var response = await context.CompleteAsync(LlmCategories.Reasoning, prompt, temperature: 0.3, maxCompletionTokens: 8192, cancellationToken).ConfigureAwait(false);
        var json = PipelineJsonFormatting.NormalizeJsonObject(response.Content);
        await context.SaveArtifactJsonAsync(ArtifactTypeNames.Architecture, "architecture-agent", json, cancellationToken).ConfigureAwait(false);
    }
}
