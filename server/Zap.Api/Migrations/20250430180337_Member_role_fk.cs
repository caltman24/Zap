using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zap.Api.Migrations
{
    /// <inheritdoc />
    public partial class Member_role_fk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyMembers_RoleId",
                table: "CompanyMembers");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMembers_RoleId",
                table: "CompanyMembers",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyMembers_RoleId",
                table: "CompanyMembers");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMembers_RoleId",
                table: "CompanyMembers",
                column: "RoleId",
                unique: true);
        }
    }
}
