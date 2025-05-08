using VerticalSliceArchitectureTemplate.Common.Domain.Heroes;
using VerticalSliceArchitectureTemplate.Common.Domain.Teams;
using VerticalSliceArchitectureTemplate.Common.Domain.Todos;

namespace VerticalSliceArchitectureTemplate.Common.Persistence;

[EfCoreConverter<TodoId>]
[EfCoreConverter<TeamId>]
[EfCoreConverter<HeroId>]
[EfCoreConverter<MissionId>]
internal sealed partial class VogenEfCoreConverters;