namespace Prophet.Application.UserCases.Pipeline.Projects;

public record UpdatePipelineProjectRequest(string Name, string? Description, DateOnly? ExpectedDate, bool IsActive);
