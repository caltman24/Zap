using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zap.Api.Migrations
{
    /// <inheritdoc />
    public partial class owner_update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_CompanyMembers_OwnerId",
                table: "Companies");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_AspNetUsers_OwnerId",
                table: "Companies",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_AspNetUsers_OwnerId",
                table: "Companies");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_CompanyMembers_OwnerId",
                table: "Companies",
                column: "OwnerId",
                principalTable: "CompanyMembers",
                principalColumn: "Id");
        }
    }
}
