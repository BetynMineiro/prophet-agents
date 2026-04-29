using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prophet.Application.Interfaces.Llm;

namespace Prophet.Adapters.Llm;

/// <summary>Routes each MAF category to a named provider + model (§10), with optional Development pin.</summary>
public sealed class CategoryRoutingLlmAdapter(
    IHostEnvironment environment,
    IOptions<LlmOptions> options,
    OpenAiCompatibleChatClient chat,
    AnthropicMessagesClient anthropic,
    ILogger<CategoryRoutingLlmAdapter> logger) : ILlmAdapter
{
    private const string HttpClientOpenAiCompatible = "llm-openai-compatible";
    private const string HttpClientAnthropic = "llm-anthropic";
    private readonly LlmOptions _opt = options.Value;

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrEmpty(request.Prompt))
            throw new ArgumentException("Prompt is required.", nameof(request));
        if (!LlmCategories.IsKnown(request.Category))
            throw new ArgumentException(
                $"Unknown category '{request.Category}'. Use {LlmCategories.Reasoning}, {LlmCategories.Structured}, or {LlmCategories.Research}.",
                nameof(request));

        var category = LlmCategories.Normalize(request.Category);
        var (providerKey, model) = ResolveProviderAndModel(category);

        if (!_opt.Providers.TryGetValue(providerKey, out var prov))
        {
            throw new InvalidOperationException(
                $"LLM provider '{providerKey}' is not defined under Llm:Providers (category '{category}').");
        }

        if (string.IsNullOrWhiteSpace(prov.ApiKey))
        {
            throw new InvalidOperationException(
                $"LLM provider '{providerKey}' has no ApiKey. Set Llm:Providers:{providerKey}:ApiKey.");
        }

        if (string.IsNullOrWhiteSpace(prov.BaseUrl))
        {
            throw new InvalidOperationException(
                $"LLM provider '{providerKey}' has an empty BaseUrl.");
        }

        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException($"No model/deployment resolved for category '{category}'.");

        logger.LogDebug(
            "LLM category={Category} provider={Provider} driver={Driver} model={Model}",
            category,
            providerKey,
            prov.Driver,
            model);

        return prov.Driver switch
        {
            LlmProviderDriver.OpenAiV1 => await CompleteOpenAiAsync(
                OpenAiEndpointKind.OpenAiV1,
                prov,
                model,
                request,
                cancellationToken).ConfigureAwait(false),
            LlmProviderDriver.AzureOpenAI => await CompleteOpenAiAsync(
                OpenAiEndpointKind.AzureOpenAi,
                prov,
                model,
                request,
                cancellationToken).ConfigureAwait(false),
            LlmProviderDriver.AnthropicMessages => await CompleteAnthropicAsync(
                prov,
                model,
                request,
                cancellationToken).ConfigureAwait(false),
            _ => throw new InvalidOperationException($"Unsupported LLM driver {prov.Driver}."),
        };
    }

    private async Task<LlmResponse> CompleteOpenAiAsync(
        OpenAiEndpointKind kind,
        LlmProviderOptions prov,
        string modelOrDeployment,
        LlmRequest request,
        CancellationToken cancellationToken)
    {
        var (content, reported) = await chat.CompleteAsync(
            HttpClientOpenAiCompatible,
            kind,
            prov.BaseUrl,
            prov.ApiKey!,
            prov.AzureApiVersion,
            modelOrDeployment,
            request.Prompt,
            request.Temperature,
            request.MaxCompletionTokens,
            azureOmitTemperature: kind == OpenAiEndpointKind.AzureOpenAi && prov.AzureOmitTemperature,
            cancellationToken).ConfigureAwait(false);

        return new LlmResponse { Content = content, Model = reported ?? modelOrDeployment };
    }

    private async Task<LlmResponse> CompleteAnthropicAsync(
        LlmProviderOptions prov,
        string model,
        LlmRequest request,
        CancellationToken cancellationToken)
    {
        var (content, reported) = await anthropic.CompleteAsync(
            HttpClientAnthropic,
            prov.BaseUrl,
            prov.ApiKey!,
            prov.AnthropicApiVersion,
            model,
            request.Prompt,
            request.Temperature,
            request.MaxCompletionTokens,
            cancellationToken).ConfigureAwait(false);

        return new LlmResponse { Content = content, Model = reported ?? model };
    }

    /// <summary>Resolves provider registry key + model: Development pin overrides per-category routing.</summary>
    private (string ProviderKey, string Model) ResolveProviderAndModel(string category)
    {
        var dev = _opt.Development;
        if (environment.IsDevelopment()
            && dev != null
            && !string.IsNullOrWhiteSpace(dev.PinAllToProvider))
        {
            if (string.IsNullOrWhiteSpace(dev.Model))
            {
                throw new InvalidOperationException(
                    "Llm:Development:Model is required when Llm:Development:PinAllToProvider is set.");
            }

            return (dev.PinAllToProvider.Trim(), dev.Model.Trim());
        }

        if (!_opt.Routing.TryGetValue(category, out var route)
            || string.IsNullOrWhiteSpace(route.Provider))
        {
            throw new InvalidOperationException(
                $"Configure Llm:Routing:{category} with Provider and Model.");
        }

        return (route.Provider.Trim(), route.Model.Trim());
    }
}
