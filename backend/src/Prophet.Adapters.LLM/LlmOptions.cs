namespace Prophet.Adapters.Llm;

/// <summary>Root LLM options — provider-agnostic: named <see cref="Providers"/> + per-category <see cref="Routing"/>.</summary>
public sealed class LlmOptions
{
    public const string SectionName = "Llm";

    /// <summary>Registry of API connections (ids are referenced by <see cref="LlmRouteTarget.Provider"/>).</summary>
    public Dictionary<string, LlmProviderOptions> Providers { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Keys: <see cref="LlmCategories"/> values (<c>reasoning</c>, <c>structured</c>, <c>research</c>).</summary>
    public Dictionary<string, LlmRouteTarget> Routing { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Optional: in Development, send every category to one provider+model (e.g. only DeepSeek).</summary>
    public LlmDevelopmentPinOptions? Development { get; set; }
}

/// <summary>Pin all pipeline categories to a single provider while <see cref="Microsoft.Extensions.Hosting.IHostEnvironment"/> is Development.</summary>
public sealed class LlmDevelopmentPinOptions
{
    /// <summary>Must match a key in <see cref="LlmOptions.Providers"/>.</summary>
    public string? PinAllToProvider { get; set; }

    /// <summary>Model id (OpenAI) or deployment name (Azure) or Anthropic model name.</summary>
    public string? Model { get; set; }
}

public sealed class LlmRouteTarget
{
    /// <summary>Key in <see cref="LlmOptions.Providers"/>.</summary>
    public string Provider { get; set; } = "";

    /// <summary>OpenAI model name, Azure deployment, Anthropic model id, DeepSeek model, etc.</summary>
    public string Model { get; set; } = "";
}

/// <summary>Which HTTP contract the provider speaks (extensible).</summary>
public enum LlmProviderDriver
{
    /// <summary>OpenAPI público + DeepSeek — <c>/v1/chat/completions</c>, Bearer.</summary>
    OpenAiV1 = 0,

    /// <summary>Azure OpenAI — deployment no URL, header <c>api-key</c>.</summary>
    AzureOpenAI = 1,

    /// <summary>Anthropic Messages — <c>/v1/messages</c>, headers <c>x-api-key</c> + <c>anthropic-version</c>.</summary>
    AnthropicMessages = 2,
}

public sealed class LlmProviderOptions
{
    public LlmProviderDriver Driver { get; set; } = LlmProviderDriver.OpenAiV1;

    /// <summary>API root: OpenAI <c>https://api.openai.com/v1</c>; Azure <c>https://{resource}.openai.azure.com</c>; Anthropic <c>https://api.anthropic.com</c>.</summary>
    public string BaseUrl { get; set; } = "";

    public string? ApiKey { get; set; }

    /// <summary>Azure OpenAI only — query <c>api-version</c>.</summary>
    public string AzureApiVersion { get; set; } = "2024-10-21";

    /// <summary>
    /// Azure OpenAI: when <c>true</c>, do not send <c>temperature</c> in the chat body (deployment default only).
    /// Required for some models (e.g. <c>gpt-5-nano</c>) that reject any non-default temperature value.
    /// </summary>
    public bool AzureOmitTemperature { get; set; }

    /// <summary>Anthropic only — see docs; default matches REST examples.</summary>
    public string AnthropicApiVersion { get; set; } = "2023-06-01";
}
