using System.Text.Json;
using System.Text.RegularExpressions;
using Prophet.Application.AgentPipeline;
using Prophet.Application.Interfaces.Llm;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.UserCases.Pipeline.PipelineExecution;

/// <summary>
/// Uses the LLM when available; otherwise keyword heuristics (pt/en). Maps a change request to the earliest pipeline step that must be re-run.
/// </summary>
public sealed class RefinementStartStepResolver(ILlmAdapter llmAdapter) : IRefinementStartStepResolver
{
    public async Task<int> ResolveStartStepIndexAsync(string changeSummary, CancellationToken cancellationToken = default)
    {
        var summary = changeSummary.Trim();
        if (summary.Length == 0)
            return 0;

        try
        {
            var fromLlm = await TryResolveFromLlmAsync(summary, cancellationToken).ConfigureAwait(false);
            if (fromLlm.HasValue)
                return Clamp(fromLlm.Value);
        }
        catch
        {
            // LLM disabled or failure — fall back to heuristics
        }

        return Clamp(ResolveHeuristic(summary));
    }

    private async Task<int?> TryResolveFromLlmAsync(string changeSummary, CancellationToken cancellationToken)
    {
        var stepList = string.Join(
            "\n",
            MainPipelineStepIds.StepIds.Select((id, i) => $"{i}: {id}"));

        const string JsonExample = """{"startFromStepIndex":4}""";
        var prompt =
            $"""
            You decide which step of a software generation pipeline must be re-run first after a user change request.
            Steps (0-based index, id):
            {stepList}

            Return a single JSON object only, no markdown, same shape as this example (use your chosen index):
            {JsonExample}

            Rules:
            - startFromStepIndex is the first step that must be executed again; all later steps will be re-run too.
            - If the change affects only documentation, use 8 (doc). Packaging (9) rarely needs to be the start unless the user only asks for bundle/manifest tweaks.
            - If the change is broad or unclear, use 0.
            - Valid range: 0..9.

            Change request:
            {changeSummary}
            """;

        var response = await llmAdapter
            .CompleteAsync(
                new LlmRequest
                {
                    Category = LlmCategories.Structured,
                    Prompt = prompt,
                    Temperature = 0.1,
                    MaxCompletionTokens = 256,
                },
                cancellationToken)
            .ConfigureAwait(false);

        var normalized = PipelineJsonFormatting.NormalizeJsonObject(response.Content);
        using var doc = JsonDocument.Parse(normalized);
        if (!doc.RootElement.TryGetProperty("startFromStepIndex", out var el))
            return null;

        return el.ValueKind switch
        {
            JsonValueKind.Number when el.TryGetInt32(out var n) => n,
            _ => null,
        };
    }

    private static int ResolveHeuristic(string s)
    {
        var t = s.ToLowerInvariant();

        if (Regex.IsMatch(t, @"\b(packag|manifest|bundle)\b"))
            return 9;
        if (Regex.IsMatch(t, @"\b(doc|documentação|documentation|readme)\b"))
            return 8;
        if (Regex.IsMatch(t, @"\b(mobile|flutter|ios|android|poc-mobile)\b"))
            return 7;
        if (Regex.IsMatch(t, @"\b(web|html|prototype|poc-web|ui)\b"))
            return 6;
        if (Regex.IsMatch(t, @"\b(diagram|uml|mermaid|fluxo|flow|class diagram)\b"))
            return 5;
        if (Regex.IsMatch(t, @"\b(architecture|arquitetura|camada|layer|microservice)\b"))
            return 4;
        if (Regex.IsMatch(t, @"\b(domain|model|entidade|aggregate|ddd|modelo de domínio)\b"))
            return 3;
        if (Regex.IsMatch(t, @"\b(market|mercado|competidor|pesquisa|research)\b"))
            return 2;
        if (Regex.IsMatch(t, @"\b(insight|regra|requisito|use case|caso de uso)\b"))
            return 1;
        if (Regex.IsMatch(t, @"\b(chunk|ficheiro|file|input|upload|texto)\b"))
            return 0;

        return 0;
    }

    private static int Clamp(int step)
    {
        if (step < 0)
            return 0;
        if (step >= MainPipelineStepIds.TotalSteps)
            return MainPipelineStepIds.TotalSteps - 1;
        return step;
    }
}
