using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;

namespace Prophet.Application.AgentPipeline;

/// <summary>Turns uploaded pipeline input bytes into plain text for <see cref="FileAgent"/> chunking.</summary>
public static partial class PipelineInputBinaryTextExtractor
{
    public static string Extract(byte[] bytes, string fileName)
    {
        if (bytes.Length == 0)
            return string.Empty;

        if (IsPdf(bytes))
            return TryExtractPdf(bytes, fileName);

        var ext = Path.GetExtension(fileName);

        if (string.Equals(ext, ".docx", StringComparison.OrdinalIgnoreCase))
            return TryExtractDocx(bytes, fileName);

        if (string.Equals(ext, ".doc", StringComparison.OrdinalIgnoreCase))
            return TryExtractLegacyDoc(bytes, fileName);

        return Encoding.UTF8.GetString(bytes);
    }

    private static bool IsPdf(byte[] bytes) =>
        bytes.Length >= 4 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46;

    /// <summary>
    /// Best-effort PDF text: scans content for PDF string literals used with <c>Tj</c> / <c>TJ</c> (no full PDF interpreter).
    /// Works for many "text PDFs"; image-only PDFs return a short notice.
    /// </summary>
    private static string TryExtractPdf(byte[] bytes, string fileName)
    {
        try
        {
            var latin1 = Encoding.GetEncoding("iso-8859-1");
            var s = latin1.GetString(bytes);
            var sb = new StringBuilder();
            ExtractPdfParenthesisStrings(s, sb);
            ExtractPdfHexStrings(s, sb);
            var result = sb.ToString().Trim();
            if (result.Length > 0)
                return result;

            return $"[PDF {fileName}: no extractable text found (image-only or compressed streams may not be supported).]";
        }
        catch (Exception ex)
        {
            return $"[PDF {fileName}: text extraction failed — {ex.Message}]";
        }
    }

    private static void ExtractPdfParenthesisStrings(string s, StringBuilder outText)
    {
        var i = 0;
        while (i < s.Length)
        {
            if (s[i] != '(')
            {
                i++;
                continue;
            }

            i++;
            var seg = new StringBuilder();
            while (i < s.Length)
            {
                var c = s[i];
                if (c == '\\' && i + 1 < s.Length)
                {
                    i++;
                    seg.Append(UnescapePdfChar(s[i]));
                    i++;
                    continue;
                }

                if (c == ')')
                {
                    i++;
                    break;
                }

                seg.Append(c);
                i++;
            }

            var t = seg.ToString().Trim();
            if (t.Length > 0 && IsMostlyPrintable(t))
            {
                outText.AppendLine(t);
                outText.AppendLine();
            }
        }
    }

    private static char UnescapePdfChar(char c) => c switch
    {
        'n' => '\n',
        'r' => '\r',
        't' => '\t',
        'b' => '\b',
        'f' => '\f',
        '(' => '(',
        ')' => ')',
        '\\' => '\\',
        _ => c,
    };

    private static void ExtractPdfHexStrings(string s, StringBuilder outText)
    {
        foreach (Match m in PdfHexStringRegex().Matches(s))
        {
            var hex = m.Groups[1].Value;
            if (hex.Length < 4 || hex.Length % 2 != 0)
                continue;

            try
            {
                var buffer = new byte[hex.Length / 2];
                for (var i = 0; i < buffer.Length; i++)
                    buffer[i] = byte.Parse(hex.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                // UTF-16BE with BOM is common in PDF hex strings
                var decoded = buffer.Length >= 2 && buffer[0] == 0xFE && buffer[1] == 0xFF
                    ? Encoding.BigEndianUnicode.GetString(buffer, 2, buffer.Length - 2)
                    : Encoding.UTF8.GetString(buffer);

                var t = decoded.Trim();
                if (t.Length > 0 && IsMostlyPrintable(t))
                {
                    outText.AppendLine(t);
                    outText.AppendLine();
                }
            }
            catch
            {
                /* skip malformed hex token */
            }
        }
    }

    [GeneratedRegex(@"<\s*([0-9A-Fa-f\s]+)\s*>\s*Tj", RegexOptions.CultureInvariant)]
    private static partial Regex PdfHexStringRegex();

    private static bool IsMostlyPrintable(string t)
    {
        if (t.Length == 0)
            return false;
        var bad = 0;
        foreach (var c in t)
        {
            if (c == '\uFFFD')
            {
                bad++;
                continue;
            }

            if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
                bad++;
        }

        return bad <= t.Length / 3;
    }

    private static string TryExtractDocx(byte[] bytes, string fileName)
    {
        try
        {
            using var ms = new MemoryStream(bytes, writable: false);
            using var doc = WordprocessingDocument.Open(ms, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null)
                return $"[Word {fileName}: empty document body.]";

            return body.InnerText;
        }
        catch (Exception ex)
        {
            return $"[Word {fileName}: could not read .docx — {ex.Message}]";
        }
    }

    /// <summary>Legacy binary .doc is not parsed here (no HWPF on net10 stack); upload is still allowed.</summary>
    private static string TryExtractLegacyDoc(byte[] bytes, string fileName)
    {
        var utf8Try = Encoding.UTF8.GetString(bytes);
        if (utf8Try.Length > 200 && IsMostlyPrintable(utf8Try))
            return utf8Try;

        return
            $"[Microsoft Word .doc (legacy) {fileName}: automatic text extraction is not supported. Save as .docx and upload again to chunk the document text.]";
    }
}
