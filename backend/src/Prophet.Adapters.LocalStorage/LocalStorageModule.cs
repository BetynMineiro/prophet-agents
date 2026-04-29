using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prophet.Application.Interfaces.Storage;

namespace Prophet.Adapters.LocalStorage;

public static class LocalStorageModule
{
    public static IServiceCollection AddLocalStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LocalStorageOptions>(configuration.GetSection(LocalStorageOptions.SectionName));
        services.AddSingleton<IStorageService, LocalFileSystemStorageService>();
        return services;
    }
}
