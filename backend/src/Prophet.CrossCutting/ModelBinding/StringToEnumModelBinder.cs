using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Prophet.CrossCutting.ModelBinding;

/// <summary>
/// Binds string from query/route to enum (e.g. ?role=Admin -> enum value). Case-insensitive.
/// Reusable across APIs that accept enum values as strings in query or route.
/// </summary>
public sealed class StringToEnumModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var type = Nullable.GetUnderlyingType(bindingContext.ModelType) ?? bindingContext.ModelType;
        if (!type.IsEnum)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            bindingContext.Result = ModelBindingResult.Success(bindingContext.ModelType.IsValueType ? Activator.CreateInstance(bindingContext.ModelType) : null);
            return Task.CompletedTask;
        }

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrWhiteSpace(value))
        {
            bindingContext.Result = bindingContext.ModelType.IsValueType
                ? ModelBindingResult.Success(Activator.CreateInstance(bindingContext.ModelType))
                : ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        if (Enum.TryParse(type, value, ignoreCase: true, out var result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            $"Invalid value '{value}' for {bindingContext.ModelName}. Valid values: {string.Join(", ", Enum.GetNames(type))}");
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
    }
}
