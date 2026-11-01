namespace RecipeBook.Models;

public class ValidationResult
{
    private readonly List<string> _errors = [];

    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

    public static ValidationResult Success() => new();

    public static ValidationResult Failure(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        var result = new ValidationResult();
        result.AddError(errorMessage);
        return result;
    }

    public void AddError(string errorMessage)
    {
        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            _errors.Add(errorMessage);
        }
    }

    public string GetFormattedErrors() => string.Join(Environment.NewLine, _errors);
}
