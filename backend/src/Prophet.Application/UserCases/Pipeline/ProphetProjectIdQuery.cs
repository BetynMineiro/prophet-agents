namespace Prophet.Application.UserCases.Pipeline;

/// <summary>Route/query scope: a Prophet project id (e.g. GET/DELETE project, list inputs).</summary>
public readonly record struct PipelineProjectIdQuery(Guid ProjectId);
