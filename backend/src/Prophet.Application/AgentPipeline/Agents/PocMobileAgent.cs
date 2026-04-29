using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Prophet.Application.AgentPipeline;
using Prophet.Application.Interfaces.Llm;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.AgentPipeline.Agents;

/// <summary>poc-mobile-agent → HTML + manifest (no service worker: GCS signed iframe origin cannot register sw.js reliably).</summary>
public sealed class PocMobileAgent : IPipelineAgent
{
    public string StepId => MainPipelineStepIds.StepIds[7];

    public async Task RunAsync(PipelineAgentRunContext context, CancellationToken cancellationToken = default)
    {
        var chunks = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.Chunks, cancellationToken).ConfigureAwait(false);
        var domain = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.DomainModel, cancellationToken).ConfigureAwait(false);
        var arch = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.Architecture, cancellationToken).ConfigureAwait(false);
        if (domain == null || arch == null)
            throw new InvalidOperationException("Missing domain-model or architecture artifact.");

        var body = """
            You are the poc-mobile-agent. Return a single JSON object only (no markdown fences):
            { "appName": "string", "shellHtml": "string (body fragment only, no html/head/body wrappers)" }

            The shellHtml MUST be a mobile-first, navigable single-page mock:
            - <style> with CSS: safe-area (env(safe-area-inset-*)), touch-friendly targets, bottom tab bar or top nav.
            - At least THREE "screens" (sections) with only one visible at a time (.screen + .active or equivalent).
            - A <script> with showPage(id) or similar to switch screens; optional mock counters/lists with emoji.
            - Optional: @import Google Fonts at the top of <style> (not <link>, body has no <head>).

            Do not use <img src="..."> for files not uploaded with this POC. Prefer CSS, emoji, or inline SVG.
            Escape double quotes in JSON strings as \\".
            App name, tab labels, and screen titles must align with domain-model.json and the original source excerpt when specified; do not invent unrelated product features.

            domain-model.json:

            """ + domain.ContentJson + """

            architecture.json:

            """ + arch.ContentJson;

        var prompt = PipelineSourceContextPrompt.PrependSourceAnchoring(body, chunks?.ContentJson, PipelineSourceContextPrompt.DiagramAndPocMaxChars);

        var response = await context.CompleteAsync(LlmCategories.Structured, prompt, temperature: 0.35, maxCompletionTokens: 12288, cancellationToken).ConfigureAwait(false);
        var normalized = PipelineJsonFormatting.NormalizeJsonObject(response.Content);
        var appName = "Mobile prototype";
        var shell = "<main><h1>Mobile POC</h1><p>Offline-friendly shell.</p></main>";
        try
        {
            using var doc = JsonDocument.Parse(normalized);
            var root = doc.RootElement;
            if (root.TryGetProperty("appName", out var n))
                appName = n.GetString() ?? appName;
            if (root.TryGetProperty("shellHtml", out var s))
                shell = s.GetString() ?? shell;
        }
        catch (JsonException)
        {
        }

        var safeName = HtmlEncoder.Default.Encode(appName);
        var html =
            "<!DOCTYPE html>\n<html lang=\"pt-BR\">\n<head><meta charset=\"utf-8\"/><meta name=\"viewport\" content=\"width=device-width, initial-scale=1, viewport-fit=cover\"/>\n<title>"
            + safeName
            + "</title>\n<style>html,body{margin:0;min-height:100%}</style>\n<link rel=\"manifest\" href=\"manifest.webmanifest\"/></head>\n<body>"
            + shell
            + "</body></html>";

        var manifest = JsonSerializer.Serialize(
            new { name = appName, short_name = "POC", display = "standalone", start_url = ".", background_color = "#fff" },
            PipelineJsonDefaults.SerializerOptions);

        await UploadTextAsync(context, html, "index.html", "text/html", cancellationToken).ConfigureAwait(false);
        await UploadTextAsync(context, manifest, "manifest.webmanifest", "application/manifest+json", cancellationToken).ConfigureAwait(false);
    }

    private static async Task UploadTextAsync(
        PipelineAgentRunContext context,
        string content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var uploaded = await context.UploadVersionFileAsync(
            "poc-mobile",
            ArtifactFileTypeNames.PocMobile,
            stream,
            fileName,
            contentType,
            cancellationToken).ConfigureAwait(false);
        if (uploaded == null)
            throw new InvalidOperationException($"Upload failed for mobile POC file: {fileName}.");
    }
}
