using SSW.VerticalSliceArchitecture.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Common.GraphQL.Scalars;

// TODO: New strongly typed IDs need a scalar here and a BindRuntimeType call in GraphQlExt
public sealed class HeroIdType() : VogenGuidIdType<HeroId>("HeroId")
{
    protected override HeroId FromGuid(Guid value) => HeroId.From(value);

    protected override Guid ToGuid(HeroId value) => value.Value;
}
