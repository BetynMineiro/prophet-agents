using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Prophet.CrossCutting.ResultObjects;
using Prophet.CrossCutting.Validation;

namespace Prophet.CrossCutting.Filters;

/// <summary>
/// After the action runs, if the validation error collector has errors (e.g. from validators or use cases),
/// replaces the response with 400 Bad Request and a standardized <see cref="Result{T}"/> containing the messages.
/// Controllers should prefer returning BadRequest(Result.Fail(errorCollector.GetErrors(), 400)) explicitly when
/// the use case returns ValidationFailed; this filter acts as a safety net so that any action that set errors
/// but returned 200/204 still results in 400 with messages.
/// </summary>
public class ValidationErrorsResultFilter : IAsyncResultFilter
{
    private readonly IValidationErrorCollector _errorCollector;

    public ValidationErrorsResultFilter(IValidationErrorCollector errorCollector)
    {
        _errorCollector = errorCollector;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (_errorCollector.HasErrors)
        {
            context.Result = new ObjectResult(
                Result<object>.Fail(_errorCollector.GetErrors()))
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        await next();
    }
}
