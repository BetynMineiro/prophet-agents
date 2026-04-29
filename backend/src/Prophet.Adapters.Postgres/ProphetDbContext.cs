using Microsoft.EntityFrameworkCore;
using Prophet.Domain.Entities.Pipeline;

namespace Prophet.Adapters.Postgres;

/// <summary>
/// EF Core context for Prophet MAF persistence (schema <c>prophet</c>). Migrations are owned by <c>Prophet.Api</c>.
/// </summary>
public class ProphetDbContext : DbContext
{
    public ProphetDbContext(DbContextOptions<ProphetDbContext> options) : base(options) { }

    public DbSet<PipelineProject> PipelineProjects => Set<PipelineProject>();

    public DbSet<PipelineInputDocument> PipelineInputDocuments => Set<PipelineInputDocument>();

    public DbSet<PipelineFinalArtifact> PipelineFinalArtifacts => Set<PipelineFinalArtifact>();

    public DbSet<PipelineHtmlPoc> PipelineHtmlPocs => Set<PipelineHtmlPoc>();

    public DbSet<ArtifactVersion> ArtifactVersions => Set<ArtifactVersion>();

    public DbSet<PipelineArtifact> PipelineArtifacts => Set<PipelineArtifact>();

    public DbSet<PipelineVersionFile> PipelineVersionFiles => Set<PipelineVersionFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyPipelineConfigurations();
    }
}
