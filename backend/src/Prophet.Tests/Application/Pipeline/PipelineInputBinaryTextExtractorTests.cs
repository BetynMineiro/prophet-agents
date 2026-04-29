using System.Text;
using Prophet.Application.AgentPipeline;

namespace Prophet.Tests.Application.Pipeline;

public sealed class PipelineInputBinaryTextExtractorTests
{
    [Fact]
    public void Extract_PdfWithLiteralString_findsText()
    {
        var latin1 = Encoding.GetEncoding("iso-8859-1");
        var bytes = latin1.GetBytes("%PDF-1.4\n1 0 obj<<>>endobj\n(Hello from PDF) Tj\n%%EOF");
        var text = PipelineInputBinaryTextExtractor.Extract(bytes, "sample.pdf");
        Assert.Contains("Hello from PDF", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Extract_LegacyDoc_returnsGuidance()
    {
        var bytes = new byte[] { 0xD0, 0xCF, 0x11, 0xE0 }; // compound file header, not readable as UTF-8 text
        var text = PipelineInputBinaryTextExtractor.Extract(bytes, "old.doc");
        Assert.Contains(".docx", text, StringComparison.OrdinalIgnoreCase);
    }
}
