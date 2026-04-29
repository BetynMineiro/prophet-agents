using Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;
using Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts.Validators;
using Prophet.Application.UserCases.Pipeline.ProjectInputs;


namespace Prophet.Tests.Application.Prophet.ProjectFinalArtifacts.Validators;

public sealed class UploadFinalArtifactsRequestValidatorTests
{
    private readonly UploadPipelineFinalArtifactsRequestValidator _validator = new();

    private static InputFileChunk Chunk(string name, byte[] content, string? skipReason = null) =>
        new(name, "application/octet-stream", content, skipReason);

    [Fact]
    public void Validate_WhenNull_ReturnsInvalid()
    {
        var r = _validator.Validate(null);
        Assert.False(r.IsValid);
        Assert.Contains("Request is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenProjectIdEmpty_ReturnsInvalid()
    {
        var r = _validator.Validate(new UploadPipelineFinalArtifactsRequest(Guid.Empty, [Chunk("f.txt", [1])]));
        Assert.False(r.IsValid);
        Assert.Contains("Project id is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenFilesNull_ReturnsInvalid()
    {
        var r = _validator.Validate(new UploadPipelineFinalArtifactsRequest(Guid.NewGuid(), null!));
        Assert.False(r.IsValid);
        Assert.Contains("Files are required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenFilesEmpty_ReturnsInvalid()
    {
        var r = _validator.Validate(new UploadPipelineFinalArtifactsRequest(Guid.NewGuid(), []));
        Assert.False(r.IsValid);
        Assert.Contains("At least one file is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenTooManyFiles_ReturnsInvalid()
    {
        var files = Enumerable.Range(0, FinalArtifactUploadLimits.MaxFilesPerRequest + 1)
            .Select(i => Chunk($"{i}.txt", [1]))
            .ToList();
        var r = _validator.Validate(new UploadPipelineFinalArtifactsRequest(Guid.NewGuid(), files));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.Contains("At most", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_WhenFileExceedsMaxBytes_ReturnsInvalid()
    {
        var big = new byte[FinalArtifactUploadLimits.MaxFileBytes + 1];
        var r = _validator.Validate(new UploadPipelineFinalArtifactsRequest(Guid.NewGuid(), [Chunk("big.bin", big)]));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.Contains("MB limit", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WhenFileHasSkipReason_IgnoresSizeCheck()
    {
        var big = new byte[FinalArtifactUploadLimits.MaxFileBytes + 1];
        var r = _validator.Validate(new UploadPipelineFinalArtifactsRequest(
            Guid.NewGuid(), [Chunk("big.bin", big, "skipped by controller")]));
        Assert.True(r.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_ReturnsOk()
    {
        var r = _validator.Validate(new UploadPipelineFinalArtifactsRequest(Guid.NewGuid(), [Chunk("report.pdf", [1, 2, 3])]));
        Assert.True(r.IsValid);
        Assert.Empty(r.Errors);
    }
}
