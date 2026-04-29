using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prophet.Adapters.Postgres;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.Postgres.Configurations.Pipeline;

internal sealed class PipelineHtmlPocConfiguration : IEntityTypeConfiguration<PipelineHtmlPoc>
{
    public void Configure(EntityTypeBuilder<PipelineHtmlPoc> e)
    {
        e.ToTable("ProphetProjectHtmlPocs", ProphetSchema.Name);
        e.HasKey(x => x.Id);
        e.Property(x => x.PipelineProjectId).HasColumnName("ProphetProjectId");
        e.Property(x => x.PocKind).HasConversion<int>();
        e.Property(x => x.OriginalFileName).HasMaxLength(512);
        e.Property(x => x.ContentType).HasMaxLength(256);
        e.Property(x => x.StorageObjectPath).HasMaxLength(1024);
        e.HasIndex(x => x.PipelineProjectId);
        e.HasIndex(x => new { x.PipelineProjectId, x.PocKind }).IsUnique();
        e.HasOne<PipelineProject>()
            .WithMany()
            .HasForeignKey(x => x.PipelineProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
