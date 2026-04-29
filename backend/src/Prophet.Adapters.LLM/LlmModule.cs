using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prophet.Application.Interfaces.Llm;

namespace Prophet.Adapters.Llm;

public static class LlmModule
{
    /// <summary>Registers <see cref="ILlmAdapter"/>: real routing when any provider has an API key; otherwise <see cref="DisabledLlmAdapter"/>.</summary>
    public static IServiceCollection AddLlmAdapter(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LlmOptions>(configuration.GetSection(LlmOptions.SectionName));
        services.AddHttpClient("llm-openai-compatible", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
        });
        services.AddHttpClient("llm-anthropic", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        services.AddSingleton<OpenAiCompatibleChatClient>();
        services.AddSingleton<AnthropicMessagesClient>();

        var opt = configuration.GetSection(LlmOptions.SectionName).Get<LlmOptions>() ?? new LlmOptions();
        var hasKey = false;
        foreach (var p in opt.Providers.Values)
        {
            if (!string.IsNullOrWhiteSpace(p.ApiKey))
            {
                hasKey = true;
                break;
            }
        }

        if (hasKey)
            services.AddSingleton<ILlmAdapter, CategoryRoutingLlmAdapter>();
        else
            services.AddSingleton<ILlmAdapter, DisabledLlmAdapter>();

        return services;
    }
}
