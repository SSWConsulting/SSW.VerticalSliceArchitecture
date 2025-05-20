using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.VerticalSliceArchitecture.Common.Domain.Entities;

namespace SSW.VerticalSliceArchitecture.Common.Persistence.Entities;

public class EntityNameConfiguration : AuditableConfiguration<EntityName>
{
    public override void PostConfigure(EntityTypeBuilder<EntityName> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(EntityName.NameMaxLength)
            .IsRequired();
    }
}