using System.IO;

namespace Prophet.Domain.Entities.Pipeline;

/// <summary>Allowed file extensions for pipeline text inputs (project inputs and per-version upload).</summary>
public static class PipelineTextInputFileRules
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "pdf",
        "doc",
        "docx",
        "txt", "md", "markdown", "mdx", "rst",
        "json", "jsonl",
        "xml", "xsl", "xslt", "xsd", "svg",
        "html", "htm", "xhtml",
        "css", "scss", "less",
        "csv", "tsv",
        "yaml", "yml",
        "log",
        "ini", "cfg", "conf", "toml",
        "sql",
        "adoc", "asciidoc",
        "properties",
        "sh", "bash", "zsh", "ps1",
        "py", "rb", "rs", "go", "java", "kt", "swift", "c", "h", "cpp", "hpp", "cs", "fs", "fsx",
        "js", "jsx", "ts", "tsx", "mjs", "cjs",
        "vue", "svelte",
    };

    /// <summary>Short hint for API error messages (not exhaustive).</summary>
    public const string AllowedHint =
        ".pdf, .doc, .docx, .txt, .md, .json, .xml, .csv, .yaml, .html, source code (.cs, .ts, …)";

    /// <summary>Returns true when <paramref name="fileName"/> has a non-empty extension on the allow list.</summary>
    public static bool IsAllowedExtension(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;
        var name = Path.GetFileName(fileName.Trim());
        if (string.IsNullOrEmpty(name))
            return false;
        var ext = Path.GetExtension(name);
        if (string.IsNullOrEmpty(ext))
            return false;
        ext = ext.TrimStart('.');
        return AllowedExtensions.Contains(ext);
    }
}
