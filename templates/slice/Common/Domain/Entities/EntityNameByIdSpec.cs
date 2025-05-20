namespace SSW.VerticalSliceArchitecture.Common.Domain.Entities;

// For more on the Specification Pattern see: https://www.ssw.com.au/rules/use-specification-pattern/
public sealed class EntityNameByIdSpec : SingleResultSpecification<EntityName>
{
    public EntityNameByIdSpec(EntityNameId entityNameId)
    {
        Query.Where(t => t.Id == entityNameId);
    }
}