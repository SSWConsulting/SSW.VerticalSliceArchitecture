using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Common.GraphQL.Scalars;

public sealed class TeamIdType() : VogenGuidIdType<TeamId>("TeamId")
{
    protected override TeamId FromGuid(Guid value) => TeamId.From(value);

    protected override Guid ToGuid(TeamId value) => value.Value;
}
