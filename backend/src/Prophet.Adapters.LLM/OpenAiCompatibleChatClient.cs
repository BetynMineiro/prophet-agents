using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Prophet.Adapters.Llm;

/// <summary>OpenAI Chat Completions — API pública (<c>/v1</c>) ou Azure OpenAI (<c>/openai/deployments/…</c>).</summary>
public sealed class OpenAiCompatibleChatClient(
    IHttpClientFactory httpClientFactory,
    ILogger<OpenAiCompatibleChatClient> logger)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<(string Content, string? Model)> CompleteAsync(
        string namedHttpClient,
        OpenAiEndpointKind endpointKind,
        string baseUrl,
        string apiKey,
        string azureApiVersion,
        string modelOrDeployment,
        string userPrompt,
        double temperature,
        int? maxCompletionTokens,
        bool azureOmitTemperature,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(namedHttpClient);
        var uri = BuildRequestUri(endpointKind, baseUrl, modelOrDeployment, azureApiVersion);

        using var req = new HttpRequestMessage(HttpMethod.Post, uri);
        ApplyAuthHeader(req, endpointKind, apiKey);

        if (endpointKind == OpenAiEndpointKind.AzureOpenAi)
        {
            req.Content = azureOmitTemperature
                ? JsonContent.Create(
                    new AzureChatCompletionRequestNoTemperature(
                        Messages: [new ChatMessage("user", userPrompt)],
                        MaxCompletionTokens: maxCompletionTokens),
                    options: SerializerOptions)
                : JsonContent.Create(
                    new AzureChatCompletionRequest(
                        Messages: [new ChatMessage("user", userPrompt)],
                        Temperature: temperature,
                        MaxCompletionTokens: maxCompletionTokens),
                    options: SerializerOptions);
        }
        else
        {
            var bodyPublic = new PublicChatCompletionRequest(
                Model: modelOrDeployment,
                Messages: [new ChatMessage("user", userPrompt)],
                Temperature: temperature,
                MaxCompletionTokens: maxCompletionTokens);
            req.Content = JsonContent.Create(bodyPublic, options: SerializerOptions);
        }

        using var res = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        var json = await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!res.IsSuccessStatusCode)
        {
            logger.LogWarning("LLM HTTP {StatusCode}: {Body}", (int)res.StatusCode, json);
            throw new InvalidOperationException(
                $"LLM request failed with HTTP {(int)res.StatusCode}. See logs for body.");
        }

        var parsed = JsonSerializer.Deserialize<ChatCompletionResponse>(json, SerializerOptions);
        var text = parsed?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrEmpty(text))
        {
            logger.LogWarning("LLM returned no assistant content. Body: {Body}", json);
            throw new InvalidOperationException("LLM returned an empty assistant message.");
        }

        return (text, parsed?.Model ?? modelOrDeployment);
    }

    private static string BuildRequestUri(
        OpenAiEndpointKind kind,
        string baseUrl,
        string modelOrDeployment,
        string azureApiVersion)
    {
        var root = baseUrl.TrimEnd('/');
        return kind switch
        {
            OpenAiEndpointKind.OpenAiV1 => $"{root}/chat/completions",
            OpenAiEndpointKind.AzureOpenAi =>
                $"{root}/openai/deployments/{Uri.EscapeDataString(modelOrDeployment)}/chat/completions?api-version={Uri.EscapeDataString(azureApiVersion)}",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    private static void ApplyAuthHeader(HttpRequestMessage req, OpenAiEndpointKind kind, string apiKey)
    {
        if (kind == OpenAiEndpointKind.AzureOpenAi)
            req.Headers.TryAddWithoutValidation("api-key", apiKey);
        else
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    private sealed record PublicChatCompletionRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] ChatMessage[] Messages,
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("max_completion_tokens")] int? MaxCompletionTokens);

    /// <summary>Azure: deployment in URL — do not send <c>model</c> in body.</summary>
    private sealed record AzureChatCompletionRequest(
        [property: JsonPropertyName("messages")] ChatMessage[] Messages,
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("max_completion_tokens")] int? MaxCompletionTokens);

    /// <summary>Azure: omit temperature for deployments that only accept the API default.</summary>
    private sealed record AzureChatCompletionRequestNoTemperature(
        [property: JsonPropertyName("messages")] ChatMessage[] Messages,
        [property: JsonPropertyName("max_completion_tokens")] int? MaxCompletionTokens);

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed class ChatCompletionResponse
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("choices")]
        public List<ChoiceDto>? Choices { get; set; }

        public sealed class ChoiceDto
        {
            [JsonPropertyName("message")]
            public MessageDto? Message { get; set; }

            public sealed class MessageDto
            {
                [JsonPropertyName("content")]
                public string? Content { get; set; }
            }
        }
    }
}
