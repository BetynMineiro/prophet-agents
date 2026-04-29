using Prophet.Application.UserCases.Pipeline.ProjectInputs;

namespace Prophet.Tests.Application.Prophet.ProjectInputs;

public sealed class InputFileNameHelperTests
{
    [Theory]
    [InlineData(null, "file")]
    [InlineData("", "file")]
    [InlineData("   ", "file")]
    public void SanitizeOriginalFileName_WhenNullOrWhitespace_ReturnsFile(string? name, string expected)
    {
        Assert.Equal(expected, InputFileNameHelper.SanitizeOriginalFileName(name));
    }

    [Fact]
    public void SanitizeOriginalFileName_UsesFileNameOnly_FromRelativePath()
    {
        var combined = Path.Combine("sub", "doc.txt");
        Assert.Equal("doc.txt", InputFileNameHelper.SanitizeOriginalFileName(combined));
    }

    [Fact]
    public void SanitizeOriginalFileName_ReplacesInvalidFileNameChars_FromOs()
    {
        var ch = '\0';
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            if (c != '\0')
            {
                ch = c;
                break;
            }
        }
        var name = $"prefix{ch}suffix.txt";
        var result = InputFileNameHelper.SanitizeOriginalFileName(name);
        Assert.DoesNotContain(ch, result);
        Assert.EndsWith(".txt", result, StringComparison.Ordinal);
    }

    [Fact]
    public void SanitizeOriginalFileName_TruncatesVeryLongName()
    {
        var longName = new string('x', 500) + ".txt";
        var result = InputFileNameHelper.SanitizeOriginalFileName(longName);
        Assert.Equal(400, result.Length);
        Assert.StartsWith("xx", result, StringComparison.Ordinal);
    }
}
