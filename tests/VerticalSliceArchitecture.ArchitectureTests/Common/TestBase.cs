using System.Reflection;
using VerticalSliceArchitectureTemplate.Common.Domain.Base;
using VerticalSliceArchitectureTemplate.Common.Interfaces;
using VerticalSliceArchitectureTemplate.Common.Persistence;

namespace VerticalSliceArchitecture.ArchTests.Common;

public abstract class TestBase
{
    protected static readonly Assembly DomainAssembly = typeof(AggregateRoot<>).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(ApplicationDbContext).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(IWebApiMarker).Assembly;
}