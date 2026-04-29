using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prophet.Adapters.Postgres;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.Postgres.Configurations.Pipeline;

internal sealed class ArtifactVersionConfiguration : IEntityTypeConfiguration<ArtifactVersion>
{
    public void Configure(EntityTypeBuilder<ArtifactVersion> e)
    {
        e.ToTable("ProphetArtifactVersions", ProphetSchema.Name);
        e.HasKey(x => x.Id);
        e.Property(x => x.PipelineProjectId).HasColumnName("ProphetProjectId");
        e.Property(x => x.ChangeSummary).HasMaxLength(8192);
        e.Property(x => x.PipelineError).HasMaxLength(8192);
        e.HasIndex(x => new { x.PipelineProjectId, x.VersionNumber }).IsUnique();
        e.HasIndex(x => x.ParentVersionId);
        e.HasOne<PipelineProject>()
            .WithMany()
            .HasForeignKey(x => x.PipelineProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        e.HasOne<ArtifactVersion>()
            .WithMany()
            .HasForeignKey(x => x.ParentVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
