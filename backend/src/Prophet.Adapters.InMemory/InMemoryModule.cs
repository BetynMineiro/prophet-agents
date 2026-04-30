using Microsoft.Extensions.DependencyInjection;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Interfaces.Storage;

namespace Prophet.Adapters.InMemory;

public static class InMemoryModule
{
    public static IServiceCollection AddInMemoryStores(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryPipelineProjectStore>();
        services.AddSingleton<IPipelineProjectStore>(sp => sp.GetRequiredService<InMemoryPipelineProjectStore>());
        services.AddSingleton<InMemoryPipelineExecutionStore>();
        services.AddSingleton<IPipelineExecutionStore>(sp => sp.GetRequiredService<InMemoryPipelineExecutionStore>());
        services.AddSingleton<IPipelineInputDocumentStore, InMemoryPipelineInputDocumentStore>();
        services.AddSingleton<IPipelineFinalArtifactStore, InMemoryPipelineFinalArtifactStore>();
        services.AddSingleton<IPipelineHtmlPocStore, InMemoryPipelineHtmlPocStore>();
        services.AddSingleton<IStorageService, InMemoryStorageService>();
        return services;
    }
}
