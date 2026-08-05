using SSW.VerticalSliceArchitecture.Common.Interfaces;

namespace Seeder;

public class SeederUserService : ICurrentUserService
{
    public string? UserId => "Seeder";
}
