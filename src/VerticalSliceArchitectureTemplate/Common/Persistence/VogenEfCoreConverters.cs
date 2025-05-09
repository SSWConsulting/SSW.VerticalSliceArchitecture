using VerticalSliceArchitectureTemplate.Common.Domain.Heroes;
using VerticalSliceArchitectureTemplate.Common.Domain.Teams;

namespace VerticalSliceArchitectureTemplate.Common.Persistence;

[EfCoreConverter<TeamId>]
[EfCoreConverter<HeroId>]
[EfCoreConverter<MissionId>]
internal sealed partial class VogenEfCoreConverters;