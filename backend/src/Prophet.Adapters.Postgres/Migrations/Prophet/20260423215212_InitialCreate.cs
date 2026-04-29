using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prophet.Adapters.Postgres.Migrations.Prophet
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "prophet");

            migrationBuilder.CreateTable(
                name: "ProphetProjects",
                schema: "prophet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    ExpectedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProphetProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProphetArtifactVersions",
                schema: "prophet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProphetProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    ParentVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangeSummary = table.Column<string>(type: "character varying(8192)", maxLength: 8192, nullable: true),
                    PipelineStatus = table.Column<int>(type: "integer", nullable: false),
                    CurrentStepIndex = table.Column<int>(type: "integer", nullable: false),
                    PipelineError = table.Column<string>(type: "character varying(8192)", maxLength: 8192, nullable: true),
                    PipelineStartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PipelineCompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProphetArtifactVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProphetArtifactVersions_ProphetArtifactVersions_ParentVersi~",
                        column: x => x.ParentVersionId,
                        principalSchema: "prophet",
                        principalTable: "ProphetArtifactVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProphetArtifactVersions_ProphetProjects_ProphetProjectId",
                        column: x => x.ProphetProjectId,
                        principalSchema: "prophet",
                        principalTable: "ProphetProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProphetProjectFinalArtifacts",
                schema: "prophet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProphetProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StorageObjectPath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProphetProjectFinalArtifacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProphetProjectFinalArtifacts_ProphetProjects_ProphetProject~",
                        column: x => x.ProphetProjectId,
                        principalSchema: "prophet",
                        principalTable: "ProphetProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProphetProjectHtmlPocs",
                schema: "prophet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProphetProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    PocKind = table.Column<int>(type: "integer", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StorageObjectPath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProphetProjectHtmlPocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProphetProjectHtmlPocs_ProphetProjects_ProphetProjectId",
                        column: x => x.ProphetProjectId,
                        principalSchema: "prophet",
                        principalTable: "ProphetProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProphetProjectInputDocuments",
                schema: "prophet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProphetProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StorageObjectPath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProphetProjectInputDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProphetProjectInputDocuments_ProphetProjects_ProphetProject~",
                        column: x => x.ProphetProjectId,
                        principalSchema: "prophet",
                        principalTable: "ProphetProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProphetPipelineArtifacts",
                schema: "prophet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtifactType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ContentJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedByAgent = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProphetPipelineArtifacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProphetPipelineArtifacts_ProphetArtifactVersions_VersionId",
                        column: x => x.VersionId,
                        principalSchema: "prophet",
                        principalTable: "ProphetArtifactVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProphetVersionFiles",
                schema: "prophet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageObjectPath = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProphetVersionFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProphetVersionFiles_ProphetArtifactVersions_VersionId",
                        column: x => x.VersionId,
                        principalSchema: "prophet",
                        principalTable: "ProphetArtifactVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProphetArtifactVersions_ParentVersionId",
                schema: "prophet",
                table: "ProphetArtifactVersions",
                column: "ParentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProphetArtifactVersions_ProphetProjectId_VersionNumber",
                schema: "prophet",
                table: "ProphetArtifactVersions",
                columns: new[] { "ProphetProjectId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProphetPipelineArtifacts_VersionId_ArtifactType",
                schema: "prophet",
                table: "ProphetPipelineArtifacts",
                columns: new[] { "VersionId", "ArtifactType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProphetProjectFinalArtifacts_ProphetProjectId",
                schema: "prophet",
                table: "ProphetProjectFinalArtifacts",
                column: "ProphetProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProphetProjectHtmlPocs_ProphetProjectId",
                schema: "prophet",
                table: "ProphetProjectHtmlPocs",
                column: "ProphetProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProphetProjectHtmlPocs_ProphetProjectId_PocKind",
                schema: "prophet",
                table: "ProphetProjectHtmlPocs",
                columns: new[] { "ProphetProjectId", "PocKind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProphetProjectInputDocuments_ProphetProjectId",
                schema: "prophet",
                table: "ProphetProjectInputDocuments",
                column: "ProphetProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProphetProjects_DeletedAtUtc",
                schema: "prophet",
                table: "ProphetProjects",
                column: "DeletedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ProphetVersionFiles_VersionId",
                schema: "prophet",
                table: "ProphetVersionFiles",
                column: "VersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProphetPipelineArtifacts",
                schema: "prophet");

            migrationBuilder.DropTable(
                name: "ProphetProjectFinalArtifacts",
                schema: "prophet");

            migrationBuilder.DropTable(
                name: "ProphetProjectHtmlPocs",
                schema: "prophet");

            migrationBuilder.DropTable(
                name: "ProphetProjectInputDocuments",
                schema: "prophet");

            migrationBuilder.DropTable(
                name: "ProphetVersionFiles",
                schema: "prophet");

            migrationBuilder.DropTable(
                name: "ProphetArtifactVersions",
                schema: "prophet");

            migrationBuilder.DropTable(
                name: "ProphetProjects",
                schema: "prophet");
        }
    }
}
