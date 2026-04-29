using Prophet.Application.UserCases.Pipeline.ProjectInputs;
using Prophet.Application.UserCases.Pipeline.ProjectInputs.Validators;

namespace Prophet.Tests.Application.Prophet.ProjectInputs.Validators;

public sealed class UploadPipelineInputDocumentsRequestValidatorTests
{
    private readonly UploadPipelineInputDocumentsRequestValidator _validator = new();

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
        var r = _validator.Validate(new UploadPipelineInputDocumentsRequest(Guid.Empty, [Chunk("a.txt", [1])]));
        Assert.False(r.IsValid);
        Assert.Contains("Project id is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenFilesNull_ReturnsInvalid()
    {
        var r = _validator.Validate(new UploadPipelineInputDocumentsRequest(Guid.NewGuid(), null!));
        Assert.False(r.IsValid);
        Assert.Contains("Files are required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenFilesEmpty_ReturnsInvalid()
    {
        var r = _validator.Validate(new UploadPipelineInputDocumentsRequest(Guid.NewGuid(), []));
        Assert.False(r.IsValid);
        Assert.Contains("At least one file is required.", r.Errors);
    }

    [Fact]
    public void Validate_WhenTooManyFiles_ReturnsInvalid()
    {
        var files = Enumerable.Range(0, UploadPipelineInputDocumentsRequestValidator.MaxFilesPerRequest + 1)
            .Select(i => Chunk($"{i}.txt", [1]))
            .ToList();
        var r = _validator.Validate(new UploadPipelineInputDocumentsRequest(Guid.NewGuid(), files));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.Contains("At most", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_WhenFileExceedsMaxBytes_ReturnsInvalid()
    {
        var big = new byte[UploadPipelineInputDocumentsUseCase.MaxFileBytes + 1];
        var r = _validator.Validate(new UploadPipelineInputDocumentsRequest(Guid.NewGuid(), [Chunk("big.txt", big)]));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.Contains("MB limit", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WhenExtensionNotAllowed_ReturnsInvalid()
    {
        var r = _validator.Validate(new UploadPipelineInputDocumentsRequest(Guid.NewGuid(), [Chunk("setup.exe", [1])]));
        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.Contains("extension not allowed", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WhenSkipReasonAndOverLimit_StillValid_SkipHandledInUseCase()
    {
        var big = new byte[UploadPipelineInputDocumentsUseCase.MaxFileBytes + 1];
        var r = _validator.Validate(new UploadPipelineInputDocumentsRequest(
            Guid.NewGuid(),
            [new InputFileChunk("x.bin", "application/octet-stream", big, "skipped in controller")]));
        Assert.True(r.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_ReturnsOk()
    {
        var r = _validator.Validate(new UploadPipelineInputDocumentsRequest(Guid.NewGuid(), [Chunk("ok.txt", [1, 2])]));
        Assert.True(r.IsValid);
        Assert.Empty(r.Errors);
    }

    private static InputFileChunk Chunk(string name, byte[] content) =>
        new(name, "application/octet-stream", content);
}
