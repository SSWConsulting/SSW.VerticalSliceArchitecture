using SSW.VerticalSliceArchitecture.Common.Domain.Base.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

public record Power : IValueObject
{
    public const int NameMaxLength = 50;

    // Private setters needed for EF
    public string Name
    {
        get;
        private set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(Name));
            ThrowIfGreaterThan(value.Length, NameMaxLength, nameof(Name));
            field = value;
        }
    } = null!;

    // Private setters needed for EF
    public int PowerLevel
    {
        get;
        private set
        {
            ThrowIfLessThan(value, 1, nameof(PowerLevel));
            ThrowIfGreaterThan(value, 10, nameof(PowerLevel));
            field = value;
        }
    }

    public Power(string name, int powerLevel)
    {
        Name = name;
        PowerLevel = powerLevel;
    }
}