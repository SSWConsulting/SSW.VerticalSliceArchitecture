using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VerticalSliceArchitectureTemplate.Features.Todos;

public class TodoEfConfiguration : IEntityTypeConfiguration<Todo>
{
    public void Configure(EntityTypeBuilder<Todo> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Text)
            .IsRequired();
    }
}