using SSW.VerticalSliceArchitecture.Common.Domain.Base;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

// Ensure stongly typed IDs are registered in 'VogenEfCoreConverters'
// For strongly typed IDs, check out the rule: https://www.ssw.com.au/rules/do-you-use-strongly-typed-ids/
[ValueObject<Guid>]
public readonly partial struct HeroId;

public class Hero : AggregateRoot<HeroId>
{
    public const int NameMaxLength = 100;
    public const int AliasMaxLength = 100;

    private readonly List<Power> _powers = [];

    public string Name
    {
        get;
        set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(Name));
            ThrowIfGreaterThan(value.Length, NameMaxLength, nameof(Name));
            field = value;
        }
    } = null!;

    public string Alias
    {
        get;
        set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(Alias));
            ThrowIfGreaterThan(value.Length, AliasMaxLength, nameof(Alias));
            field = value;
        }
    } = null!;

    public int PowerLevel { get; private set; }
    public TeamId? TeamId { get; private set; }

    public IReadOnlyList<Power> Powers => _powers.AsReadOnly();

    private Hero() { } // Needed for EF Core

    public static Hero Create(string name, string alias)
    {
        var hero = new Hero { Id = HeroId.From(Guid.CreateVersion7()), Name = name, Alias = alias };

        return hero;
    }

    public void UpdatePowers(IEnumerable<Power> updatedPowers)
    {
        _powers.Clear();
        PowerLevel = 0;

        foreach (var heroPowerModel in updatedPowers)
            AddPower(new Power(heroPowerModel.Name, heroPowerModel.PowerLevel));

        AddDomainEvent(new PowerLevelUpdatedEvent(this));
    }

    private void AddPower(Power power)
    {
        ThrowIfNull(power);

        if (!_powers.Contains(power))
        {
            _powers.Add(power);
        }

        PowerLevel += power.PowerLevel;
    }
}