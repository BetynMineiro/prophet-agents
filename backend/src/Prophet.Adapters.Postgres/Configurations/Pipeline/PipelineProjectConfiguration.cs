using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prophet.Adapters.Postgres;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.Postgres.Configurations.Pipeline;

internal sealed class PipelineProjectConfiguration : IEntityTypeConfiguration<PipelineProject>
{
    public void Configure(EntityTypeBuilder<PipelineProject> e)
    {
        e.ToTable("ProphetProjects", ProphetSchema.Name);
        e.HasKey(x => x.Id);
        e.Property(x => x.Name).HasMaxLength(256);
        e.Property(x => x.Description).HasMaxLength(4096);
        e.Property(x => x.ExpectedDate);
        e.HasIndex(x => x.DeletedAtUtc);
    }
}
