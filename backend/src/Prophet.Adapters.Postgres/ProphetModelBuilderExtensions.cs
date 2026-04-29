using Microsoft.EntityFrameworkCore;

namespace Prophet.Adapters.Postgres;

internal static class ProphetModelBuilderExtensions
{
    private const string PipelineConfigurationsSuffix = ".Configurations.Pipeline";

    public static void ApplyPipelineConfigurations(this ModelBuilder builder) =>
        builder.ApplyConfigurationsFromAssembly(typeof(ProphetDbContext).Assembly, IsPipelineConfigurationType);

    private static bool IsPipelineConfigurationType(Type type) =>
        type is { IsClass: true, IsAbstract: false } &&
        type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)) &&
        (type.Namespace ?? string.Empty).EndsWith(PipelineConfigurationsSuffix, StringComparison.Ordinal);
}
