using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prophet.Adapters.Postgres;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.Postgres.Configurations.Pipeline;

internal sealed class PipelineInputDocumentConfiguration : IEntityTypeConfiguration<PipelineInputDocument>
{
    public void Configure(EntityTypeBuilder<PipelineInputDocument> e)
    {
        e.ToTable("ProphetProjectInputDocuments", ProphetSchema.Name);
        e.HasKey(x => x.Id);
        e.Property(x => x.PipelineProjectId).HasColumnName("ProphetProjectId");
        e.Property(x => x.OriginalFileName).HasMaxLength(512);
        e.Property(x => x.ContentType).HasMaxLength(256);
        e.Property(x => x.StorageObjectPath).HasMaxLength(1024);
        e.HasIndex(x => x.PipelineProjectId);
        e.HasOne<PipelineProject>()
            .WithMany()
            .HasForeignKey(x => x.PipelineProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
