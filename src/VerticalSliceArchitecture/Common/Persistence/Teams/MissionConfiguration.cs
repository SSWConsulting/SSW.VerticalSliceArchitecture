using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Common.Persistence.Heroes;

namespace SSW.VerticalSliceArchitecture.Common.Persistence.Teams;

public class MissionConfiguration : AuditableConfiguration<Mission>
{
    public override void PostConfigure(EntityTypeBuilder<Mission> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Description)
            .HasMaxLength(Mission.DescriptionMaxLength)
            .IsRequired();
    }
}