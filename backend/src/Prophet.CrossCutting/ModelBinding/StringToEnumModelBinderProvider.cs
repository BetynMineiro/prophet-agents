using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Prophet.CrossCutting.ModelBinding;

/// <summary>
/// Provides StringToEnumModelBinder for any enum type so query/route string values bind to enum.
/// Reusable across APIs; register with ModelBinderProviders.Insert(0, new StringToEnumModelBinderProvider()).
/// </summary>
public sealed class StringToEnumModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var type = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
        return type.IsEnum ? new StringToEnumModelBinder() : null;
    }
}
