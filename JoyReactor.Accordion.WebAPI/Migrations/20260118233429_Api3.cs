using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoyReactor.Accordion.Workers.Migrations;

/// <inheritdoc />
public partial class Api3 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<Guid>(
            name: "ApiId",
            table: "ParsedTags",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldDefaultValue: new Guid("00000001-0000-0000-0000-000000000000"));

        migrationBuilder.AlterColumn<Guid>(
            name: "ApiId",
            table: "ParsedPosts",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldDefaultValue: new Guid("00000001-0000-0000-0000-000000000000"));

        migrationBuilder.AlterColumn<Guid>(
            name: "ApiId",
            table: "EmptyTags",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldDefaultValue: new Guid("00000001-0000-0000-0000-000000000000"));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<Guid>(
            name: "ApiId",
            table: "ParsedTags",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000001-0000-0000-0000-000000000000"),
            oldClrType: typeof(Guid),
            oldType: "uuid");

        migrationBuilder.AlterColumn<Guid>(
            name: "ApiId",
            table: "ParsedPosts",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000001-0000-0000-0000-000000000000"),
            oldClrType: typeof(Guid),
            oldType: "uuid");

        migrationBuilder.AlterColumn<Guid>(
            name: "ApiId",
            table: "EmptyTags",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000001-0000-0000-0000-000000000000"),
            oldClrType: typeof(Guid),
            oldType: "uuid");
    }
}
