using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zap.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class StoredFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketAttachments_CompanyMembers_OwnerId",
                table: "TicketAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketAttachments_Tickets_TicketId1",
                table: "TicketAttachments");

            migrationBuilder.DropIndex(
                name: "IX_TicketAttachments_TicketId1",
                table: "TicketAttachments");

            migrationBuilder.DropColumn(
                name: "StoreKey",
                table: "TicketAttachments");

            migrationBuilder.DropColumn(
                name: "TicketId1",
                table: "TicketAttachments");

            migrationBuilder.RenameColumn(
                name: "StoreUrl",
                table: "TicketAttachments",
                newName: "FileId");

            migrationBuilder.CreateTable(
                name: "StoredFiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Visibility = table.Column<string>(type: "text", nullable: false),
                    BucketName = table.Column<string>(type: "text", nullable: false),
                    ObjectKey = table.Column<string>(type: "text", nullable: false),
                    OriginalFileName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoredFiles_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoredFiles_CompanyMembers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "CompanyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachments_FileId",
                table: "TicketAttachments",
                column: "FileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_CompanyId",
                table: "StoredFiles",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_OwnerId",
                table: "StoredFiles",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAttachments_CompanyMembers_OwnerId",
                table: "TicketAttachments",
                column: "OwnerId",
                principalTable: "CompanyMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAttachments_StoredFiles_FileId",
                table: "TicketAttachments",
                column: "FileId",
                principalTable: "StoredFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketAttachments_CompanyMembers_OwnerId",
                table: "TicketAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketAttachments_StoredFiles_FileId",
                table: "TicketAttachments");

            migrationBuilder.DropTable(
                name: "StoredFiles");

            migrationBuilder.DropIndex(
                name: "IX_TicketAttachments_FileId",
                table: "TicketAttachments");

            migrationBuilder.RenameColumn(
                name: "FileId",
                table: "TicketAttachments",
                newName: "StoreUrl");

            migrationBuilder.AddColumn<string>(
                name: "StoreKey",
                table: "TicketAttachments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TicketId1",
                table: "TicketAttachments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachments_TicketId1",
                table: "TicketAttachments",
                column: "TicketId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAttachments_CompanyMembers_OwnerId",
                table: "TicketAttachments",
                column: "OwnerId",
                principalTable: "CompanyMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAttachments_Tickets_TicketId1",
                table: "TicketAttachments",
                column: "TicketId1",
                principalTable: "Tickets",
                principalColumn: "Id");
        }
    }
}
