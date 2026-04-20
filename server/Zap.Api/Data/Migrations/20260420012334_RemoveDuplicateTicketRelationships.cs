using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zap.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDuplicateTicketRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketComments_Tickets_TicketId1",
                table: "TicketComments");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketHistories_Tickets_TicketId1",
                table: "TicketHistories");

            migrationBuilder.DropIndex(
                name: "IX_TicketHistories_TicketId1",
                table: "TicketHistories");

            migrationBuilder.DropIndex(
                name: "IX_TicketComments_TicketId1",
                table: "TicketComments");

            migrationBuilder.DropColumn(
                name: "TicketId1",
                table: "TicketHistories");

            migrationBuilder.DropColumn(
                name: "TicketId1",
                table: "TicketComments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TicketId1",
                table: "TicketHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketId1",
                table: "TicketComments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistories_TicketId1",
                table: "TicketHistories",
                column: "TicketId1");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_TicketId1",
                table: "TicketComments",
                column: "TicketId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComments_Tickets_TicketId1",
                table: "TicketComments",
                column: "TicketId1",
                principalTable: "Tickets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketHistories_Tickets_TicketId1",
                table: "TicketHistories",
                column: "TicketId1",
                principalTable: "Tickets",
                principalColumn: "Id");
        }
    }
}
