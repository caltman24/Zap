using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zap.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class avatarKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoKey",
                table: "Companies",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarKey",
                table: "AspNetUsers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoKey",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "AvatarKey",
                table: "AspNetUsers");
        }
    }
}
