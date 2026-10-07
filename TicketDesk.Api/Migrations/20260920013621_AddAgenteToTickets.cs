using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketDesk.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAgenteToTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgenteId",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AgenteId",
                table: "Tickets",
                column: "AgenteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Usuarios_AgenteId",
                table: "Tickets",
                column: "AgenteId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Usuarios_AgenteId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_AgenteId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AgenteId",
                table: "Tickets");
        }
    }
}
