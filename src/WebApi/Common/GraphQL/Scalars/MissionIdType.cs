using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Common.GraphQL.Scalars;

public sealed class MissionIdType() : VogenGuidIdType<MissionId>("MissionId")
{
    protected override MissionId FromGuid(Guid value) => MissionId.From(value);

    protected override Guid ToGuid(MissionId value) => value.Value;
}
