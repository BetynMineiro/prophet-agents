using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Prophet.Application.AgentPipeline;
using Prophet.Application.Interfaces.Llm;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Application.AgentPipeline.Agents;

/// <summary>poc-web-agent → <c>index.html</c> via backend storage (§4 #7).</summary>
public sealed class PocWebAgent : IPipelineAgent
{
    public string StepId => MainPipelineStepIds.StepIds[6];

    public async Task RunAsync(PipelineAgentRunContext context, CancellationToken cancellationToken = default)
    {
        var chunks = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.Chunks, cancellationToken).ConfigureAwait(false);
        var domain = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.DomainModel, cancellationToken).ConfigureAwait(false);
        var arch = await context.Store.GetArtifactByTypeAsync(context.VersionId, ArtifactTypeNames.Architecture, cancellationToken).ConfigureAwait(false);
        if (domain == null || arch == null)
            throw new InvalidOperationException("Missing domain-model or architecture artifact.");

        var promptBody = """
            You are the poc-web-agent. Produce a navigable single-file web POC (one index.html), like a polished product demo.

            Return ONE JSON object only (no markdown fences):
            { "title": "string", "htmlBody": "string" }

            The "htmlBody" is the FULL content to place inside <body> (no outer <html>/<head>/<body> wrappers). It MUST include, in order:
            1) A <style> block with rich CSS: :root variables, layout (grid/flex), responsive typography, dark or light theme matching the domain.
            2) A fixed or sticky <nav> with links/buttons that call a navigation function (e.g. showPage('home')).
            3) At least THREE distinct full-width "screens" or sections (e.g. home, catalog/list, detail/cart/checkout — adapt labels to the domain). Only ONE screen visible at a time: use classes like .page { display:none } and .page.active { display:block } or equivalent.
            4) A short <script> at the end implementing showPage(id) (or go(name)) that toggles .active / visibility, and any lightweight mock interactions (e.g. add-to-cart counters with emoji icons, tab switches). Keep JS vanilla, no external JS libraries.
            5) Optional typography: use @import url('https://fonts.googleapis.com/css2?...') at the top of the first <style> block (body fragment has no <head>, so do not rely on <link> for fonts). Do NOT load arbitrary third-party scripts.

            Quality bar: the result should feel like a small navigable SPA mock (similar to a multi-section ecommerce or SaaS landing demo), not a single static paragraph.

            Hard constraints:
            - Do not use <img src="..."> for files that are not uploaded; only this HTML is stored. Use emoji, CSS gradients, Unicode symbols, or inline SVG for visuals.
            - Escape double quotes inside htmlBody strings for JSON (use \\").
            - Screen titles, nav labels, and domain wording must align with domain-model.json and the original source excerpt when they specify product names or flows; do not invent competing products or unrelated features.

            domain-model.json:

            """ + domain.ContentJson + """

            architecture.json:

            """ + arch.ContentJson;

        var prompt = PipelineSourceContextPrompt.PrependSourceAnchoring(promptBody, chunks?.ContentJson, PipelineSourceContextPrompt.DiagramAndPocMaxChars);

        var response = await context.CompleteAsync(LlmCategories.Structured, prompt, temperature: 0.35, maxCompletionTokens: 16384, cancellationToken).ConfigureAwait(false);
        var normalized = PipelineJsonFormatting.NormalizeJsonObject(response.Content);
        var title = "Web prototype";
        var body = "<main><p>Prototype</p></main>";
        try
        {
            using var doc = JsonDocument.Parse(normalized);
            var root = doc.RootElement;
            if (root.TryGetProperty("title", out var t))
                title = t.GetString() ?? title;
            if (root.TryGetProperty("htmlBody", out var b))
                body = b.GetString() ?? body;
        }
        catch (JsonException)
        {
            // keep defaults
        }

        var safeTitle = HtmlEncoder.Default.Encode(title);
        var fullHtml =
            "<!DOCTYPE html>\n<html lang=\"pt-BR\">\n<head><meta charset=\"utf-8\"/><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"/><title>"
            + safeTitle
            + "</title>\n<style>html,body{margin:0;min-height:100%}</style></head>\n<body>"
            + body
            + "</body></html>";

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(fullHtml));
        var uploaded = await context.UploadVersionFileAsync(
            "poc-web",
            ArtifactFileTypeNames.PocWeb,
            stream,
            "index.html",
            "text/html",
            cancellationToken).ConfigureAwait(false);
        if (uploaded == null)
            throw new InvalidOperationException("Upload failed for web POC (storage or validation).");
    }
}
