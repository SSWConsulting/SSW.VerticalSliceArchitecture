using VerticalSliceArchitecture.ArchTests.Common;
using VerticalSliceArchitectureTemplate.Common.Persistence;

namespace VerticalSliceArchitecture.ArchTests;

public class Presentation : TestBase
{
    // private static readonly Type DbContext = typeof(ApplicationDbContext);

    // [Fact]
    // public void Endpoints_ShouldNotReferenceDbContext()
    // {
    //     var types = Types
    //         .InAssembly(PresentationAssembly)
    //         .That()
    //         .HaveNameEndingWith("Endpoints");
    //
    //     var result = types
    //         .ShouldNot()
    //         .HaveDependencyOnAny(DbContext.FullName, IDbContext.FullName)
    //         .GetResult();
    //
    //     result.Should().BeSuccessful();
    // }
}