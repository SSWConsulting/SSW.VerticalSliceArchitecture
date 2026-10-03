namespace SSW.VerticalSliceArchitecture.Common.Validation;

public static class ValidationRules
{
    /// <summary>
    /// Fails when a strongly typed id is uninitialised, or wraps an empty <see cref="Guid"/>.
    /// </summary>
    /// <remarks>
    /// FluentValidation's <c>NotEmpty</c> compares against <c>default(TId)</c>, and an id built from
    /// <c>Guid.Empty</c> is not that — it is an initialised value object holding a meaningless
    /// value, so the rule would pass. The uninitialised check has to come first: reading
    /// <c>Value</c> on a default instance throws.
    /// </remarks>
    public static IRuleBuilderOptions<T, TId> NotEmptyId<T, TId>(
        this IRuleBuilder<T, TId> ruleBuilder,
        Func<TId, Guid> value)
        where TId : struct
    {
        ThrowIfNull(value);

        return ruleBuilder
            .Must(id => !EqualityComparer<TId>.Default.Equals(id, default) && value(id) != Guid.Empty)
            .WithMessage("'{PropertyName}' must not be empty.");
    }
}
