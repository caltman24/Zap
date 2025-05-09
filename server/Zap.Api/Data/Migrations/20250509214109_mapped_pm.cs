using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zap.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class mapped_pm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyMemberProject_CompanyMembers_AssignedMembersId",
                table: "CompanyMemberProject");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyMemberProject_Projects_AssignedProjectsId",
                table: "CompanyMemberProject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyMemberProject",
                table: "CompanyMemberProject");

            migrationBuilder.RenameTable(
                name: "CompanyMemberProject",
                newName: "ProjectMembers");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyMemberProject_AssignedProjectsId",
                table: "ProjectMembers",
                newName: "IX_ProjectMembers_AssignedProjectsId");

            migrationBuilder.AddColumn<string>(
                name: "ProjectManagerId",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectMembers",
                table: "ProjectMembers",
                columns: new[] { "AssignedMembersId", "AssignedProjectsId" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectManagerId",
                table: "Projects",
                column: "ProjectManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_CompanyMembers_AssignedMembersId",
                table: "ProjectMembers",
                column: "AssignedMembersId",
                principalTable: "CompanyMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_Projects_AssignedProjectsId",
                table: "ProjectMembers",
                column: "AssignedProjectsId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_CompanyMembers_ProjectManagerId",
                table: "Projects",
                column: "ProjectManagerId",
                principalTable: "CompanyMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_CompanyMembers_AssignedMembersId",
                table: "ProjectMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_Projects_AssignedProjectsId",
                table: "ProjectMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_CompanyMembers_ProjectManagerId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ProjectManagerId",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectMembers",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "ProjectManagerId",
                table: "Projects");

            migrationBuilder.RenameTable(
                name: "ProjectMembers",
                newName: "CompanyMemberProject");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectMembers_AssignedProjectsId",
                table: "CompanyMemberProject",
                newName: "IX_CompanyMemberProject_AssignedProjectsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyMemberProject",
                table: "CompanyMemberProject",
                columns: new[] { "AssignedMembersId", "AssignedProjectsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyMemberProject_CompanyMembers_AssignedMembersId",
                table: "CompanyMemberProject",
                column: "AssignedMembersId",
                principalTable: "CompanyMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyMemberProject_Projects_AssignedProjectsId",
                table: "CompanyMemberProject",
                column: "AssignedProjectsId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
