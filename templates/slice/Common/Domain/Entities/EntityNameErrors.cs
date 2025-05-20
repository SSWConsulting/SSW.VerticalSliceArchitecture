namespace SSW.VerticalSliceArchitecture.Common.Domain.Entities;

public static class EntityNameErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "EntityName.NotFound",
        "EntityName is not found");
}