using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoyReactor.Accordion.Workers.Migrations
{
    /// <inheritdoc />
    public partial class CrawlerTaskEmptyTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrawlerTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostLineType = table.Column<int>(type: "integer", nullable: false),
                    PageFrom = table.Column<int>(type: "integer", nullable: true),
                    PageTo = table.Column<int>(type: "integer", nullable: true),
                    PageCurrent = table.Column<int>(type: "integer", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlerTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrawlerTasks_ParsedTags_TagId",
                        column: x => x.TagId,
                        principalTable: "ParsedTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmptyTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NumberId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmptyTags", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrawlerTasks_TagId",
                table: "CrawlerTasks",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_EmptyTags_NumberId",
                table: "EmptyTags",
                column: "NumberId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrawlerTasks");

            migrationBuilder.DropTable(
                name: "EmptyTags");
        }
    }
}
