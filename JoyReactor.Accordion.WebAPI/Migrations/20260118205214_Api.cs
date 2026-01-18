using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JoyReactor.Accordion.Workers.Migrations;

/// <inheritdoc />
public partial class Api : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Apis",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                GraphQlEndpointUrl = table.Column<string>(type: "text", nullable: false),
                RootTagNames = table.Column<string[]>(type: "text[]", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Apis", x => x.Id);
            });

        migrationBuilder.InsertData(
            table: "Apis",
            columns: new[] { "Id", "GraphQlEndpointUrl", "Name", "RootTagNames" },
            values: new object[,]
            {
                { new Guid("00000001-0000-0000-0000-000000000000"), "https://api.joyreactor.cc/graphql", "JoyReactor.cc", new[] { "общее", "Комиксы", "гифки", "art", "песочница", "котэ", "story", "geek", "видео", "фэндомы", "Эротика", "Игры", "anon", "политика", "разное", "секретные разделы", "artist", "Мемы", "Азиатка", "Porn Model", "cosplay" } },
                { new Guid("00000002-0000-0000-0000-000000000000"), "https://api.joyreactor.com/graphql", "JoyReactor.com", new[] { "general", "comics", "gif", "art", "sandbox", "cats", "story", "geek", "video", "fandoms", "erotic", "games", "anon", "politics", "artist", "memes", "asian girl", "Porn Model", "cosplay" } }
            });

        migrationBuilder.CreateIndex(
            name: "IX_Apis_GraphQlEndpointUrl",
            table: "Apis",
            column: "GraphQlEndpointUrl",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Apis_Name",
            table: "Apis",
            column: "Name",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Apis");
    }
}
