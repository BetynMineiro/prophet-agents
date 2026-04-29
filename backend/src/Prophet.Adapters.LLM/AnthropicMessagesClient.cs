using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Prophet.Adapters.Llm;

/// <summary>Anthropic Messages API — distinct from OpenAI-compatible vendors.</summary>
public sealed class AnthropicMessagesClient(
    IHttpClientFactory httpClientFactory,
    ILogger<AnthropicMessagesClient> logger)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<(string Content, string? Model)> CompleteAsync(
        string namedHttpClient,
        string baseUrl,
        string apiKey,
        string anthropicApiVersion,
        string model,
        string userPrompt,
        double temperature,
        int? maxCompletionTokens,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(namedHttpClient);
        var uri = $"{baseUrl.TrimEnd('/')}/v1/messages";
        var body = new AnthropicRequest(
            Model: model,
            MaxTokens: maxCompletionTokens ?? 4096,
            Temperature: temperature,
            Messages: [new AnthropicUserMessage("user", userPrompt)]);

        using var req = new HttpRequestMessage(HttpMethod.Post, uri);
        req.Headers.TryAddWithoutValidation("x-api-key", apiKey);
        req.Headers.TryAddWithoutValidation("anthropic-version", anthropicApiVersion);
        req.Content = JsonContent.Create(body, options: SerializerOptions);

        using var res = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        var json = await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!res.IsSuccessStatusCode)
        {
            logger.LogWarning("Anthropic HTTP {StatusCode}: {Body}", (int)res.StatusCode, json);
            throw new InvalidOperationException(
                $"Anthropic request failed with HTTP {(int)res.StatusCode}. See logs for body.");
        }

        var parsed = JsonSerializer.Deserialize<AnthropicResponse>(json, SerializerOptions);
        var text = ExtractText(parsed);
        if (string.IsNullOrEmpty(text))
        {
            logger.LogWarning("Anthropic returned no text content. Body: {Body}", json);
            throw new InvalidOperationException("Anthropic returned no assistant text.");
        }

        return (text, parsed?.Model);
    }

    private static string? ExtractText(AnthropicResponse? parsed)
    {
        var blocks = parsed?.Content;
        if (blocks == null)
            return null;
        foreach (var b in blocks)
        {
            if (string.Equals(b.Type, "text", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(b.Text))
                return b.Text;
        }

        return null;
    }

    private sealed record AnthropicRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("max_tokens")] int MaxTokens,
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("messages")] AnthropicUserMessage[] Messages);

    private sealed record AnthropicUserMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed class AnthropicResponse
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("content")]
        public List<ContentBlock>? Content { get; set; }

        public sealed class ContentBlock
        {
            [JsonPropertyName("type")]
            public string? Type { get; set; }

            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }
    }
}
