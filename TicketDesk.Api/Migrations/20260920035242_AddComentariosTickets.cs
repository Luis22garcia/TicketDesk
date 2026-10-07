using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketDesk.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddComentariosTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComentariosTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComentariosTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComentariosTickets_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComentariosTickets_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$W3kN.0enOmDHwL1C1VZDb.rUfzjiz5YjTwyBYzR6dWV/uBaui33BG");

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosTickets_TicketId",
                table: "ComentariosTickets",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosTickets_UsuarioId",
                table: "ComentariosTickets",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComentariosTickets");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$W3kN.0enOmDHLwL1C1VZDb.rUfzjiz5YjTwyBYzR6dWV/uBaui33BG");
        }
    }
}
