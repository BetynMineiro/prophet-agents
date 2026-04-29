namespace Prophet.Application.UserCases.Pipeline.ProjectInputs;

/// <summary>Single file payload for Prophet project input upload (small files; validated in use case).</summary>
/// <param name="SkipReason">When set, the file is not uploaded; this message is returned as the failure reason.</param>
public sealed record InputFileChunk(string OriginalFileName, string ContentType, byte[] Content, string? SkipReason = null);
