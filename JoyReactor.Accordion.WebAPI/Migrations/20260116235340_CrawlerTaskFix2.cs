using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoyReactor.Accordion.Workers.Migrations
{
    /// <inheritdoc />
    public partial class CrawlerTaskFix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "CrawlerTasks");

            migrationBuilder.DropColumn(
                name: "IsIndefinite",
                table: "CrawlerTasks");

            migrationBuilder.DropColumn(
                name: "PageFrom",
                table: "CrawlerTasks");

            migrationBuilder.RenameColumn(
                name: "PageTo",
                table: "CrawlerTasks",
                newName: "PageLast");

            migrationBuilder.AlterColumn<int>(
                name: "PageCurrent",
                table: "CrawlerTasks",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PageLast",
                table: "CrawlerTasks",
                newName: "PageTo");

            migrationBuilder.AlterColumn<int>(
                name: "PageCurrent",
                table: "CrawlerTasks",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "CrawlerTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsIndefinite",
                table: "CrawlerTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PageFrom",
                table: "CrawlerTasks",
                type: "integer",
                nullable: true);
        }
    }
}
