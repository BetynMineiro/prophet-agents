namespace Prophet.Adapters.Llm;

/// <summary>HTTP shape for <see cref="OpenAiCompatibleChatClient"/> (OpenAI public vs Azure).</summary>
public enum OpenAiEndpointKind
{
    OpenAiV1 = 0,
    AzureOpenAi = 1,
}
