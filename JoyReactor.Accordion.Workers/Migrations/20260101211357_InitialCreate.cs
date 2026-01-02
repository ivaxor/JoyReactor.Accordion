using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoyReactor.Accordion.Workers.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParsedBandCamps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlbumId = table.Column<string>(type: "text", nullable: false),
                    PostAttributeEmbededId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedBandCamps", x => x.Id);
                    table.UniqueConstraint("AK_ParsedBandCamps_PostAttributeEmbededId", x => x.PostAttributeEmbededId);
                });

            migrationBuilder.CreateTable(
                name: "ParsedCoubs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<string>(type: "text", nullable: false),
                    PostAttributeEmbededId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedCoubs", x => x.Id);
                    table.UniqueConstraint("AK_ParsedCoubs_PostAttributeEmbededId", x => x.PostAttributeEmbededId);
                });

            migrationBuilder.CreateTable(
                name: "ParsedPost",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NumberId = table.Column<int>(type: "integer", nullable: false),
                    ContentVersion = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedPost", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParsedSoundClouds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UrlPath = table.Column<string>(type: "text", nullable: false),
                    PostAttributeEmbededId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedSoundClouds", x => x.Id);
                    table.UniqueConstraint("AK_ParsedSoundClouds_PostAttributeEmbededId", x => x.PostAttributeEmbededId);
                });

            migrationBuilder.CreateTable(
                name: "ParsedTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MainTagId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    NumberId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PostCount = table.Column<int>(type: "integer", nullable: false),
                    SubscriberCount = table.Column<int>(type: "integer", nullable: false),
                    SubTagsCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParsedTags_ParsedTags_MainTagId",
                        column: x => x.MainTagId,
                        principalTable: "ParsedTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParsedTags_ParsedTags_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ParsedTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParsedVimeos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<string>(type: "text", nullable: false),
                    PostAttributeEmbededId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedVimeos", x => x.Id);
                    table.UniqueConstraint("AK_ParsedVimeos_PostAttributeEmbededId", x => x.PostAttributeEmbededId);
                });

            migrationBuilder.CreateTable(
                name: "ParsedYoutubes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VideoId = table.Column<string>(type: "text", nullable: false),
                    PostAttributeEmbededId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedYoutubes", x => x.Id);
                    table.UniqueConstraint("AK_ParsedYoutubes_PostAttributeEmbededId", x => x.PostAttributeEmbededId);
                });

            migrationBuilder.CreateTable(
                name: "ParsedPostAttributePictures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttributeId = table.Column<int>(type: "integer", nullable: false),
                    ImageId = table.Column<int>(type: "integer", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsVectorCreated = table.Column<bool>(type: "boolean", nullable: false),
                    VectorLastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedPostAttributePictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParsedPostAttributePictures_ParsedPost_PostId",
                        column: x => x.PostId,
                        principalTable: "ParsedPost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParsedPostAttributeEmbeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    BandCampId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoubId = table.Column<Guid>(type: "uuid", nullable: true),
                    SoundCloudId = table.Column<Guid>(type: "uuid", nullable: true),
                    VimeoId = table.Column<Guid>(type: "uuid", nullable: true),
                    YouTubeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsedPostAttributeEmbeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParsedPostAttributeEmbeds_ParsedBandCamps_BandCampId",
                        column: x => x.BandCampId,
                        principalTable: "ParsedBandCamps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParsedPostAttributeEmbeds_ParsedCoubs_CoubId",
                        column: x => x.CoubId,
                        principalTable: "ParsedCoubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParsedPostAttributeEmbeds_ParsedPost_PostId",
                        column: x => x.PostId,
                        principalTable: "ParsedPost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParsedPostAttributeEmbeds_ParsedSoundClouds_SoundCloudId",
                        column: x => x.SoundCloudId,
                        principalTable: "ParsedSoundClouds",
                        principalColumn: "PostAttributeEmbededId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParsedPostAttributeEmbeds_ParsedVimeos_VimeoId",
                        column: x => x.VimeoId,
                        principalTable: "ParsedVimeos",
                        principalColumn: "PostAttributeEmbededId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParsedPostAttributeEmbeds_ParsedYoutubes_YouTubeId",
                        column: x => x.YouTubeId,
                        principalTable: "ParsedYoutubes",
                        principalColumn: "PostAttributeEmbededId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParsedBandCamps_AlbumId",
                table: "ParsedBandCamps",
                column: "AlbumId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedCoubs_VideoId",
                table: "ParsedCoubs",
                column: "VideoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPost_NumberId",
                table: "ParsedPost",
                column: "NumberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPostAttributeEmbeds_BandCampId",
                table: "ParsedPostAttributeEmbeds",
                column: "BandCampId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPostAttributeEmbeds_CoubId",
                table: "ParsedPostAttributeEmbeds",
                column: "CoubId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPostAttributeEmbeds_PostId_BandCampId_CoubId_SoundClo~",
                table: "ParsedPostAttributeEmbeds",
                columns: new[] { "PostId", "BandCampId", "CoubId", "SoundCloudId", "VimeoId", "YouTubeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPostAttributeEmbeds_SoundCloudId",
                table: "ParsedPostAttributeEmbeds",
                column: "SoundCloudId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPostAttributeEmbeds_VimeoId",
                table: "ParsedPostAttributeEmbeds",
                column: "VimeoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPostAttributeEmbeds_YouTubeId",
                table: "ParsedPostAttributeEmbeds",
                column: "YouTubeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPostAttributePictures_IsVectorCreated",
                table: "ParsedPostAttributePictures",
                column: "IsVectorCreated");

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPostAttributePictures_PostId_AttributeId",
                table: "ParsedPostAttributePictures",
                columns: new[] { "PostId", "AttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedSoundClouds_UrlPath",
                table: "ParsedSoundClouds",
                column: "UrlPath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedTags_MainTagId",
                table: "ParsedTags",
                column: "MainTagId");

            migrationBuilder.CreateIndex(
                name: "IX_ParsedTags_Name",
                table: "ParsedTags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedTags_NumberId",
                table: "ParsedTags",
                column: "NumberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedTags_ParentId",
                table: "ParsedTags",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ParsedVimeos_VideoId",
                table: "ParsedVimeos",
                column: "VideoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParsedYoutubes_VideoId",
                table: "ParsedYoutubes",
                column: "VideoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParsedPostAttributeEmbeds");

            migrationBuilder.DropTable(
                name: "ParsedPostAttributePictures");

            migrationBuilder.DropTable(
                name: "ParsedTags");

            migrationBuilder.DropTable(
                name: "ParsedBandCamps");

            migrationBuilder.DropTable(
                name: "ParsedCoubs");

            migrationBuilder.DropTable(
                name: "ParsedSoundClouds");

            migrationBuilder.DropTable(
                name: "ParsedVimeos");

            migrationBuilder.DropTable(
                name: "ParsedYoutubes");

            migrationBuilder.DropTable(
                name: "ParsedPost");
        }
    }
}
