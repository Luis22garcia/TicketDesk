using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketDesk.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHistorialTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistorialTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstadoAnterior = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoNuevo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comentario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialTickets_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistorialTickets_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "Descripcion",
                value: "Problemas relacionados con equipos físicos.");

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "Descripcion",
                value: "Problemas de conexión y red.");

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "Descripcion",
                value: "Problemas de acceso, cuentas y contraseñas.");

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "Descripcion",
                value: "Otros problemas de soporte técnico.");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$kcb1D1sDTsRbqwVpQg7icueQsvDYNX/xgP/5aOcRQ3dElOgUQU8Yu");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$6iWHfL3aWA.WBZGjJqd9KO.Wbwnk32kgPUBRiVFhGZQ1wKiRFOlCW");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$W3kN.0enOmDHLwL1C1VZDb.rUfzjiz5YjTwyBYzR6dWV/uBaui33BG");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialTickets_TicketId",
                table: "HistorialTickets",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialTickets_UsuarioId",
                table: "HistorialTickets",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialTickets");

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "Descripcion",
                value: "Problemas relacionados con equipos y dispositivos.");

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "Descripcion",
                value: "Problemas relacionados con conectividad y red.");

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "Descripcion",
                value: "Problemas relacionados con cuentas y acceso.");

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "Descripcion",
                value: "Otros problemas de soporte.");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "TEMP_HASH_ADMIN");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "TEMP_HASH_SOPORTE");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "TEMP_HASH_USUARIO");
        }
    }
}
