namespace Prophet.CrossCutting.Validation;

/// <summary>
/// Service to communicate validation/business rule errors along the flow (use case → controller/job).
/// </summary>
public interface IValidationErrorCollector
{
    void AddError(string message);
    bool HasErrors { get; }
    IReadOnlyList<string> GetErrors();
    void Clear();
}
