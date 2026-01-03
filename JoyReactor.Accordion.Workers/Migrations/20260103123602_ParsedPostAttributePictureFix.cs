using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoyReactor.Accordion.Workers.Migrations
{
    /// <inheritdoc />
    public partial class ParsedPostAttributePictureFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VectorLastUpdatedAt",
                table: "ParsedPostAttributePictures");

            migrationBuilder.RenameColumn(
                name: "ImageId",
                table: "ParsedPostAttributePictures",
                newName: "ImageType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageType",
                table: "ParsedPostAttributePictures",
                newName: "ImageId");

            migrationBuilder.AddColumn<DateTime>(
                name: "VectorLastUpdatedAt",
                table: "ParsedPostAttributePictures",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
