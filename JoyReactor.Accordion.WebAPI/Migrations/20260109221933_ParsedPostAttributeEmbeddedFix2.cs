using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoyReactor.Accordion.Workers.Migrations
{
    /// <inheritdoc />
    public partial class ParsedPostAttributeEmbeddedFix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParsedPostAttributeEmbeds_BandCampId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.DropIndex(
                name: "IX_ParsedPostAttributeEmbeds_CoubId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.DropIndex(
                name: "IX_ParsedPostAttributeEmbeds_SoundCloudId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.DropIndex(
                name: "IX_ParsedPostAttributeEmbeds_VimeoId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.DropIndex(
                name: "IX_ParsedPostAttributeEmbeds_YouTubeId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPostAttributeEmbeds_BandCampId",
                table: "ParsedPostAttributeEmbeds",
                column: "BandCampId");

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPostAttributeEmbeds_CoubId",
                table: "ParsedPostAttributeEmbeds",
                column: "CoubId");

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPostAttributeEmbeds_SoundCloudId",
                table: "ParsedPostAttributeEmbeds",
                column: "SoundCloudId");

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPostAttributeEmbeds_VimeoId",
                table: "ParsedPostAttributeEmbeds",
                column: "VimeoId");

            migrationBuilder.CreateIndex(
                name: "IX_ParsedPostAttributeEmbeds_YouTubeId",
                table: "ParsedPostAttributeEmbeds",
                column: "YouTubeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParsedPostAttributeEmbeds_BandCampId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.DropIndex(
                name: "IX_ParsedPostAttributeEmbeds_CoubId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.DropIndex(
                name: "IX_ParsedPostAttributeEmbeds_SoundCloudId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.DropIndex(
                name: "IX_ParsedPostAttributeEmbeds_VimeoId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.DropIndex(
                name: "IX_ParsedPostAttributeEmbeds_YouTubeId",
                table: "ParsedPostAttributeEmbeds");

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
        }
    }
}
