using SSW.VerticalSliceArchitecture.Common.Domain.Base;

namespace SSW.VerticalSliceArchitecture.Common.Domain.Entities;

// For strongly typed IDs, check out the rule: https://www.ssw.com.au/rules/do-you-use-strongly-typed-ids/
[ValueObject<Guid>]
public readonly partial struct EntityNameId;

public class EntityName : AggregateRoot<EntityNameId>
{
    public const int NameMaxLength = 100;
    
    private string _name = null!;

    public string Name
    {
        get => _name;
        set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(Name));
            ThrowIfGreaterThan(value.Length, NameMaxLength, nameof(Name));
            _name = value;
        }
    }

    private EntityName() { } // Needed for EF Core

    public static EntityName Create(string name)
    {
        var entityName = new EntityName { Id = EntityNameId.From(Guid.CreateVersion7()), Name = name };

        return entityName;
    }
}