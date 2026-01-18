using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoyReactor.Accordion.Workers.Migrations;

/// <inheritdoc />
public partial class Api2 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Name",
            table: "Apis",
            newName: "HostName");

        migrationBuilder.RenameIndex(
            name: "IX_Apis_Name",
            table: "Apis",
            newName: "IX_Apis_HostName");

        migrationBuilder.AddColumn<Guid>(
            name: "ApiId",
            table: "ParsedTags",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000001-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<Guid>(
            name: "ApiId",
            table: "ParsedPosts",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000001-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<Guid>(
            name: "ApiId",
            table: "EmptyTags",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000001-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<int>(
            name: "Priority",
            table: "Apis",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.UpdateData(
            table: "Apis",
            keyColumn: "Id",
            keyValue: new Guid("00000001-0000-0000-0000-000000000000"),
            columns: new[] { "HostName", "Priority" },
            values: new object[] { "joyreactor.cc", 0 });

        migrationBuilder.UpdateData(
            table: "Apis",
            keyColumn: "Id",
            keyValue: new Guid("00000002-0000-0000-0000-000000000000"),
            columns: new[] { "HostName", "Priority" },
            values: new object[] { "joyreactor.com", -1 });

        migrationBuilder.CreateIndex(
            name: "IX_ParsedTags_ApiId",
            table: "ParsedTags",
            column: "ApiId");

        migrationBuilder.CreateIndex(
            name: "IX_ParsedPosts_ApiId",
            table: "ParsedPosts",
            column: "ApiId");

        migrationBuilder.CreateIndex(
            name: "IX_EmptyTags_ApiId",
            table: "EmptyTags",
            column: "ApiId");

        migrationBuilder.CreateIndex(
            name: "IX_Apis_Priority",
            table: "Apis",
            column: "Priority",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_EmptyTags_Apis_ApiId",
            table: "EmptyTags",
            column: "ApiId",
            principalTable: "Apis",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_ParsedPosts_Apis_ApiId",
            table: "ParsedPosts",
            column: "ApiId",
            principalTable: "Apis",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_ParsedTags_Apis_ApiId",
            table: "ParsedTags",
            column: "ApiId",
            principalTable: "Apis",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_EmptyTags_Apis_ApiId",
            table: "EmptyTags");

        migrationBuilder.DropForeignKey(
            name: "FK_ParsedPosts_Apis_ApiId",
            table: "ParsedPosts");

        migrationBuilder.DropForeignKey(
            name: "FK_ParsedTags_Apis_ApiId",
            table: "ParsedTags");

        migrationBuilder.DropIndex(
            name: "IX_ParsedTags_ApiId",
            table: "ParsedTags");

        migrationBuilder.DropIndex(
            name: "IX_ParsedPosts_ApiId",
            table: "ParsedPosts");

        migrationBuilder.DropIndex(
            name: "IX_EmptyTags_ApiId",
            table: "EmptyTags");

        migrationBuilder.DropIndex(
            name: "IX_Apis_Priority",
            table: "Apis");

        migrationBuilder.DropColumn(
            name: "ApiId",
            table: "ParsedTags");

        migrationBuilder.DropColumn(
            name: "ApiId",
            table: "ParsedPosts");

        migrationBuilder.DropColumn(
            name: "ApiId",
            table: "EmptyTags");

        migrationBuilder.DropColumn(
            name: "Priority",
            table: "Apis");

        migrationBuilder.RenameColumn(
            name: "HostName",
            table: "Apis",
            newName: "Name");

        migrationBuilder.RenameIndex(
            name: "IX_Apis_HostName",
            table: "Apis",
            newName: "IX_Apis_Name");

        migrationBuilder.UpdateData(
            table: "Apis",
            keyColumn: "Id",
            keyValue: new Guid("00000001-0000-0000-0000-000000000000"),
            column: "Name",
            value: "JoyReactor.cc");

        migrationBuilder.UpdateData(
            table: "Apis",
            keyColumn: "Id",
            keyValue: new Guid("00000002-0000-0000-0000-000000000000"),
            column: "Name",
            value: "JoyReactor.com");
    }
}
