using SSW.VerticalSliceArchitecture.Common.Domain.Base;

namespace SSW.VerticalSliceArchitecture.Common.Domain.Teams;

// For strongly typed IDs, check out the rule: https://www.ssw.com.au/rules/do-you-use-strongly-typed-ids/
[ValueObject<Guid>]
public readonly partial struct MissionId;

public class Mission : Entity<MissionId>
{
    private string _description = null!;
    public const int DescriptionMaxLength = 500;

    public string Description
    {
        get => _description;
        private set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(Description));
            ThrowIfGreaterThan(value.Length, DescriptionMaxLength, nameof(Description));
            _description = value;
        }
    }

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