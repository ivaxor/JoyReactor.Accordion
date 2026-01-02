using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoyReactor.Accordion.Workers.Migrations
{
    /// <inheritdoc />
    public partial class ParsedAttributeFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParsedPostAttributeEmbeds_ParsedSoundClouds_SoundCloudId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.DropForeignKey(
                name: "FK_ParsedPostAttributeEmbeds_ParsedVimeos_VimeoId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.DropForeignKey(
                name: "FK_ParsedPostAttributeEmbeds_ParsedYoutubes_YouTubeId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ParsedYoutubes_PostAttributeEmbededId",
                table: "ParsedYoutubes");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ParsedVimeos_PostAttributeEmbededId",
                table: "ParsedVimeos");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ParsedSoundClouds_PostAttributeEmbededId",
                table: "ParsedSoundClouds");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ParsedCoubs_PostAttributeEmbededId",
                table: "ParsedCoubs");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ParsedBandCamps_PostAttributeEmbededId",
                table: "ParsedBandCamps");

            migrationBuilder.DropColumn(
                name: "PostAttributeEmbededId",
                table: "ParsedYoutubes");

            migrationBuilder.DropColumn(
                name: "PostAttributeEmbededId",
                table: "ParsedVimeos");

            migrationBuilder.DropColumn(
                name: "PostAttributeEmbededId",
                table: "ParsedSoundClouds");

            migrationBuilder.DropColumn(
                name: "PostAttributeEmbededId",
                table: "ParsedCoubs");

            migrationBuilder.DropColumn(
                name: "PostAttributeEmbededId",
                table: "ParsedBandCamps");

            migrationBuilder.AddForeignKey(
                name: "FK_ParsedPostAttributeEmbeds_ParsedSoundClouds_SoundCloudId",
                table: "ParsedPostAttributeEmbeds",
                column: "SoundCloudId",
                principalTable: "ParsedSoundClouds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParsedPostAttributeEmbeds_ParsedVimeos_VimeoId",
                table: "ParsedPostAttributeEmbeds",
                column: "VimeoId",
                principalTable: "ParsedVimeos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParsedPostAttributeEmbeds_ParsedYoutubes_YouTubeId",
                table: "ParsedPostAttributeEmbeds",
                column: "YouTubeId",
                principalTable: "ParsedYoutubes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParsedPostAttributeEmbeds_ParsedSoundClouds_SoundCloudId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.DropForeignKey(
                name: "FK_ParsedPostAttributeEmbeds_ParsedVimeos_VimeoId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.DropForeignKey(
                name: "FK_ParsedPostAttributeEmbeds_ParsedYoutubes_YouTubeId",
                table: "ParsedPostAttributeEmbeds");

            migrationBuilder.AddColumn<Guid>(
                name: "PostAttributeEmbededId",
                table: "ParsedYoutubes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PostAttributeEmbededId",
                table: "ParsedVimeos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PostAttributeEmbededId",
                table: "ParsedSoundClouds",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PostAttributeEmbededId",
                table: "ParsedCoubs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PostAttributeEmbededId",
                table: "ParsedBandCamps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ParsedYoutubes_PostAttributeEmbededId",
                table: "ParsedYoutubes",
                column: "PostAttributeEmbededId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ParsedVimeos_PostAttributeEmbededId",
                table: "ParsedVimeos",
                column: "PostAttributeEmbededId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ParsedSoundClouds_PostAttributeEmbededId",
                table: "ParsedSoundClouds",
                column: "PostAttributeEmbededId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ParsedCoubs_PostAttributeEmbededId",
                table: "ParsedCoubs",
                column: "PostAttributeEmbededId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ParsedBandCamps_PostAttributeEmbededId",
                table: "ParsedBandCamps",
                column: "PostAttributeEmbededId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParsedPostAttributeEmbeds_ParsedSoundClouds_SoundCloudId",
                table: "ParsedPostAttributeEmbeds",
                column: "SoundCloudId",
                principalTable: "ParsedSoundClouds",
                principalColumn: "PostAttributeEmbededId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParsedPostAttributeEmbeds_ParsedVimeos_VimeoId",
                table: "ParsedPostAttributeEmbeds",
                column: "VimeoId",
                principalTable: "ParsedVimeos",
                principalColumn: "PostAttributeEmbededId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParsedPostAttributeEmbeds_ParsedYoutubes_YouTubeId",
                table: "ParsedPostAttributeEmbeds",
                column: "YouTubeId",
                principalTable: "ParsedYoutubes",
                principalColumn: "PostAttributeEmbededId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
