using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prophet.Adapters.Postgres;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.Postgres.Configurations.Pipeline;

internal sealed class PipelineArtifactConfiguration : IEntityTypeConfiguration<PipelineArtifact>
{
    public void Configure(EntityTypeBuilder<PipelineArtifact> e)
    {
        e.ToTable("ProphetPipelineArtifacts", ProphetSchema.Name);
        e.HasKey(x => x.Id);
        e.Property(x => x.ArtifactType).HasMaxLength(64).IsRequired();
        e.Property(x => x.ContentJson).HasColumnType("jsonb");
        e.Property(x => x.CreatedByAgent).HasMaxLength(128);
        e.HasIndex(x => new { x.VersionId, x.ArtifactType }).IsUnique();
        e.HasOne<ArtifactVersion>()
            .WithMany()
            .HasForeignKey(x => x.VersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
