using SSW.VerticalSliceArchitecture.Common.Domain.Base.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

public record Power : IValueObject
{
    public const int NameMaxLength = 50;

    private string _name = null!;
    private int _powerLevel;

    // Private setters needed for EF
    public string Name
    {
        get => _name;
        private set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(Name));
            ThrowIfGreaterThan(value.Length, NameMaxLength, nameof(Name));
            _name = value;
        }
    }

    // Private setters needed for EF
    public int PowerLevel
    {
        get => _powerLevel;
        private set
        {
            ThrowIfLessThan(value, 1, nameof(PowerLevel));
            ThrowIfGreaterThan(value, 10, nameof(PowerLevel));
            _powerLevel = value;
        }
    }

    public Power(string name, int powerLevel)
    {
        Name = name;
        PowerLevel = powerLevel;
    }
}