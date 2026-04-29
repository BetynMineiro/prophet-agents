using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prophet.Adapters.Postgres;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.Postgres.Configurations.Pipeline;

internal sealed class PipelineVersionFileConfiguration : IEntityTypeConfiguration<PipelineVersionFile>
{
    public void Configure(EntityTypeBuilder<PipelineVersionFile> e)
    {
        e.ToTable("ProphetVersionFiles", ProphetSchema.Name);
        e.HasKey(x => x.Id);
        e.Property(x => x.FileType).HasMaxLength(64).IsRequired();
        e.Property(x => x.StorageObjectPath).HasMaxLength(2048).IsRequired();
        e.Property(x => x.OriginalFileName).HasMaxLength(512);
        e.HasIndex(x => x.VersionId);
        e.HasOne<ArtifactVersion>()
            .WithMany()
            .HasForeignKey(x => x.VersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
