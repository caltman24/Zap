using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zap.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class commentFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketComments_Tickets_SenderId",
                table: "TicketComments");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "TicketComments",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "TicketId1",
                table: "TicketComments",
                type: "text",
                nullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketComments_Tickets_TicketId1",
                table: "TicketComments");

            migrationBuilder.DropIndex(
                name: "IX_TicketComments_TicketId1",
                table: "TicketComments");

            migrationBuilder.DropColumn(
                name: "TicketId1",
                table: "TicketComments");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "TicketComments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComments_Tickets_SenderId",
                table: "TicketComments",
                column: "SenderId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
