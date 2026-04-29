using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prophet.Application.Interfaces.Llm;
using Prophet.CrossCutting.ResultObjects;

namespace Prophet.Api.Controllers.Dev;

/// <summary>
/// Endpoint temporário para smoke-test do <see cref="ILlmAdapter"/> (ex.: Azure OpenAI com deployment nano/DeepSeek).
/// Disponível apenas quando <c>IHostEnvironment.IsDevelopment()</c> é true; em outros ambientes devolve 404.
/// </summary>
[ApiController]
[Route("v1/prophet/dev/llm")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public sealed class LlmDevController(ILlmAdapter llmAdapter, IHostEnvironment environment) : ControllerBase
{
    /// <summary>Envia uma mensagem ao LLM configurado (routing/categoria conforme appsettings <c>Llm:</c>).</summary>
    [HttpPost("complete")]
    [ProducesResponseType(typeof(Result<LlmDevCompleteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<LlmDevCompleteResponse>>> Complete(
        [FromBody] LlmDevCompleteRequest body,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
            return NotFound();

        if (string.IsNullOrWhiteSpace(body.Message))
            return BadRequest(Result<LlmDevCompleteResponse>.Fail("message is required.", StatusCodes.Status400BadRequest));

        var category = string.IsNullOrWhiteSpace(body.Category)
            ? LlmCategories.Reasoning
            : LlmCategories.Normalize(body.Category);

        if (!LlmCategories.IsKnown(category))
        {
            return BadRequest(Result<LlmDevCompleteResponse>.Fail(
                $"category must be one of: {LlmCategories.Reasoning}, {LlmCategories.Structured}, {LlmCategories.Research}.",
                StatusCodes.Status400BadRequest));
        }

        var request = new LlmRequest
        {
            Category = category,
            Prompt = body.Message.Trim(),
            Temperature = body.Temperature ?? 1.0,
            MaxCompletionTokens = body.MaxCompletionTokens,
        };

        try
        {
            var response = await llmAdapter.CompleteAsync(request, cancellationToken).ConfigureAwait(false);
            return Ok(Result<LlmDevCompleteResponse>.Ok(new LlmDevCompleteResponse
            {
                Content = response.Content,
                Model = response.Model,
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway,
                Result<LlmDevCompleteResponse>.Fail($"LLM call failed: {ex.Message}", StatusCodes.Status502BadGateway));
        }
    }
}

public sealed class LlmDevCompleteRequest
{
    /// <summary>Mensagem de utilizador enviada ao modelo.</summary>
    [Required(AllowEmptyStrings = false)]
    public string Message { get; init; } = "";

    /// <summary>Opcional: <see cref="LlmCategories"/> (predefinido: reasoning).</summary>
    public string? Category { get; init; }

    public double? Temperature { get; init; }

    public int? MaxCompletionTokens { get; init; }
}

public sealed class LlmDevCompleteResponse
{
    public required string Content { get; init; }
    public string? Model { get; init; }
}
