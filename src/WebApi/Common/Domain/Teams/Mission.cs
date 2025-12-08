using SSW.VerticalSliceArchitecture.Common.Domain.Base;

namespace SSW.VerticalSliceArchitecture.Common.Domain.Teams;

// Ensure stongly typed IDs are registered in 'VogenEfCoreConverters'
// For strongly typed IDs, check out the rule: https://www.ssw.com.au/rules/do-you-use-strongly-typed-ids/
[ValueObject<Guid>]
public readonly partial struct MissionId;

public class Mission : Entity<MissionId>
{
    public const int DescriptionMaxLength = 500;

    public string Description
    {
        get;
        private set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(Description));
            ThrowIfGreaterThan(value.Length, DescriptionMaxLength, nameof(Description));
            field = value;
        }
    } = null!;

    public MissionStatus Status { get; private set; }

    private Mission() { } // Needed for EF Core

    // NOTE: Internal so that missions can only be created by the aggregate
    internal static Mission Create(string description)
    {
        return new Mission
        {
            Id = MissionId.From(Guid.CreateVersion7()),
            Description = description,
            Status = MissionStatus.InProgress
        };
    }

    internal ErrorOr<Success> Complete()
    {
        if (Status == MissionStatus.Complete)
        {
            return MissionErrors.AlreadyCompleted;
        }

        Status = MissionStatus.Complete;

        return new Success();
    }
}