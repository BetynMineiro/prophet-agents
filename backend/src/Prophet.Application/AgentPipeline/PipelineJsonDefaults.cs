using System.Text.Json;

namespace Prophet.Application.AgentPipeline;

internal static class PipelineJsonDefaults
{
    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };
}
