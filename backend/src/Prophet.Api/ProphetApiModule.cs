using Microsoft.AspNetCore.Mvc;
using Prophet.Adapters.InMemory;
using Prophet.Adapters.Llm;
using Prophet.Application;
using Prophet.Application.Options;
using Prophet.CrossCutting;
using Prophet.CrossCutting.Filters;
using Prophet.CrossCutting.ModelBinding;

namespace Prophet.Api;

public static class ProphetApiModule
{
    public static void ConfigureFilters(this MvcOptions options)
    {
        options.Filters.Add<ValidationErrorsResultFilter>();
        options.Filters.Add(new ProducesAttribute("application/json"));
        options.ModelBinderProviders.Insert(0, new StringToEnumModelBinderProvider());
    }

    extension(IServiceCollection services)
    {
        public void ConfigureProphetApiServicesLayer(IConfiguration configuration, IHostEnvironment environment)
        {
            services.ConfigureLogging(environment);
            services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
            services.AddPipelineApplication();
            services.AddLlmAdapter(configuration);
            services.AddInMemoryStores();
        }
    }
}
