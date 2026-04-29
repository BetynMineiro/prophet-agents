namespace Prophet.Application.UserCases.Pipeline.Projects;

/// <param name="IsActive">When null (omitted in JSON), treated as active.</param>
public record CreatePipelineProjectRequest(string Name, string? Description, DateOnly? ExpectedDate, bool? IsActive = null);
