using System.Text.Json;
using HotChocolate.Features;
using HotChocolate.Language;
using HotChocolate.Text.Json;

namespace SSW.VerticalSliceArchitecture.Common.GraphQL.Scalars;

/// <summary>
/// Base for a GraphQL scalar over a Vogen value object that wraps a <see cref="Guid"/>.
/// </summary>
/// <remarks>
/// A distinct scalar per identifier, rather than <c>UUID</c> everywhere, is the schema-level half of
/// strongly typed IDs: a client cannot pass a <c>TeamId</c> where a <c>HeroId</c> belongs, and the
/// mistake is caught while the query is validated.
/// <para>
/// Vogen generates <c>From</c> and <c>Value</c> as members of the struct rather than as an interface,
/// so the two conversions stay abstract here and each derived scalar supplies them.
/// </para>
/// </remarks>
public abstract class VogenGuidIdType<TId>(string name) : ScalarType<TId, StringValueNode>(name)
    where TId : struct
{
    protected abstract TId FromGuid(Guid value);

    protected abstract Guid ToGuid(TId value);

    protected override TId OnCoerceInputLiteral(StringValueNode valueLiteral)
    {
        if (!Guid.TryParse(valueLiteral.Value, out var value))
            throw new LeafCoercionException($"The specified value is not a valid {Name}.", this);

        return FromGuid(value);
    }

    protected override TId OnCoerceInputValue(JsonElement inputValue, IFeatureProvider context)
    {
        if (inputValue.ValueKind is not JsonValueKind.String || !Guid.TryParse(inputValue.GetString(), out var value))
            throw new LeafCoercionException($"The specified value is not a valid {Name}.", this);

        return FromGuid(value);
    }

    protected override void OnCoerceOutputValue(TId runtimeValue, ResultElement resultValue) =>
        resultValue.SetStringValue(ToGuid(runtimeValue).ToString("D"));

    protected override StringValueNode OnValueToLiteral(TId runtimeValue) =>
        new(ToGuid(runtimeValue).ToString("D"));
}
