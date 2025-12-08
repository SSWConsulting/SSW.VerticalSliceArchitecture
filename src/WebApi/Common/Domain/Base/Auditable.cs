using SSW.VerticalSliceArchitecture.Common.Domain.Base.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.Domain.Base;

/// <summary>
/// Tracks creation and modification of objects.
/// </summary>
public abstract class Auditable : IAuditable
{
    public const int CreatedByMaxLength = 128;
    public const int UpdatedByMaxLength = 128;

    private const string SystemUser = "System";

    public DateTimeOffset CreatedAt { get; private set; }

    public string CreatedBy
    {
        get;
        private set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(CreatedBy));
            ThrowIfGreaterThan(value.Length, CreatedByMaxLength, nameof(CreatedBy));
            field = value;
        }
    } = null!;

    public DateTimeOffset? UpdatedAt { get; private set; }

    public string? UpdatedBy
    {
        get;
        private set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(UpdatedBy));
            ThrowIfGreaterThan(value.Length, UpdatedByMaxLength, nameof(UpdatedBy));
            field = value;
        }
    }

    public void SetCreated(TimeProvider timeProvider, string? createdBy)
    {
        CreatedAt = timeProvider.GetUtcNow();
        CreatedBy = createdBy ?? SystemUser;
    }

    public void SetUpdated(TimeProvider timeProvider, string? updatedBy)
    {
        UpdatedAt = timeProvider.GetUtcNow();
        UpdatedBy = updatedBy ?? SystemUser;
    }
}