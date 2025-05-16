using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Common.Persistence.Heroes;

namespace SSW.VerticalSliceArchitecture.Common.Persistence.Teams;

public class TeamConfiguration : AuditableConfiguration<Team>
{
    public override void PostConfigure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(Team.NameMaxLength)
            .IsRequired();

        builder.HasMany(t => t.Missions)
            .WithOne()
            .IsRequired();

        builder.HasMany(t => t.Heroes)
            .WithOne()
            .HasForeignKey(h => h.TeamId)
            .IsRequired(false);
    }
}