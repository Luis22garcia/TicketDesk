using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TicketDesk.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsuariosCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Activa", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Problemas relacionados con equipos y dispositivos.", "Hardware" },
                    { 2, true, "Problemas relacionados con aplicaciones y sistemas.", "Software" },
                    { 3, true, "Problemas relacionados con conectividad y red.", "Red" },
                    { 4, true, "Problemas relacionados con cuentas y acceso.", "Acceso" },
                    { 5, true, "Otros problemas de soporte.", "Otro" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "Email", "FechaCreacion", "Nombre", "PasswordHash", "Rol" },
                values: new object[,]
                {
                    { 1, true, "admin@ticketdesk.com", new DateTime(2026, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Administrador", "TEMP_HASH_ADMIN", "Administrador" },
                    { 2, true, "soporte@ticketdesk.com", new DateTime(2026, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agente de Soporte", "TEMP_HASH_SOPORTE", "Agente" },
                    { 3, true, "usuario@ticketdesk.com", new DateTime(2026, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Usuario Demo", "TEMP_HASH_USUARIO", "Usuario" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
