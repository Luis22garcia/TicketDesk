
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketDesk.Api.Data;

namespace TicketDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Agente,Usuario")]
public class DashboardController : ControllerBase
{
    private readonly TicketDeskDbContext _context;

    public DashboardController(TicketDeskDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        // Identificar el rol autenticado.
        var esAdministrador = User.IsInRole("Administrador");
        var esAgente = User.IsInRole("Agente");
        var esUsuario = User.IsInRole("Usuario");

        // Consulta base de tickets.
        IQueryable<TicketDesk.Api.Models.Ticket> consultaTickets =
            _context.Tickets.AsNoTracking();

        // Agente y Usuario necesitan identificarse mediante su token.
        if (esAgente || esUsuario)
        {
            var idTexto =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("id");

            if (!int.TryParse(idTexto, out var id))
            {
                return Unauthorized(new
                {
                    mensaje = "No fue posible identificar al usuario del token."
                });
            }

            if (esAgente)
            {
                // El agente solo ve los tickets asignados a él.
                consultaTickets = consultaTickets
                    .Where(t => t.AgenteId == id);
            }
            else
            {
                // El usuario solo ve los tickets que él registró.
                consultaTickets = consultaTickets
                    .Where(t => t.UsuarioId == id);
            }
        }

        // Estadísticas generales según los tickets permitidos para el rol.
        var totalTickets = await consultaTickets.CountAsync();

        var ticketsAbiertos = await consultaTickets
            .CountAsync(t => t.Estado == "Abierto");

        var ticketsEnProceso = await consultaTickets
            .CountAsync(t => t.Estado == "En proceso");

        var ticketsResueltos = await consultaTickets
            .CountAsync(t => t.Estado == "Resuelto");

        var ticketsCerrados = await consultaTickets
            .CountAsync(t => t.Estado == "Cerrado");

        var ticketsAlta = await consultaTickets
            .CountAsync(t => t.Prioridad == "Alta");

        var ticketsMedia = await consultaTickets
            .CountAsync(t => t.Prioridad == "Media");

        var ticketsBaja = await consultaTickets
            .CountAsync(t => t.Prioridad == "Baja");

        // Tickets agrupados por categoría.
        var ticketsPorCategoria = await consultaTickets
            .GroupBy(t => new
            {
                t.CategoriaId,
                CategoriaNombre = t.Categoria != null
                    ? t.Categoria.Nombre
                    : "Sin categoría"
            })
            .Select(g => new
            {
                categoriaId = g.Key.CategoriaId,
                categoria = g.Key.CategoriaNombre,
                cantidad = g.Count()
            })
            .ToListAsync();

        // Tickets agrupados por estado.
        var ticketsPorEstado = await consultaTickets
            .GroupBy(t => t.Estado)
            .Select(g => new
            {
                estado = g.Key,
                cantidad = g.Count()
            })
            .OrderByDescending(x => x.cantidad)
            .ToListAsync();

        // Solo el Administrador obtiene estadísticas globales de usuarios.
        var usuariosActivos = esAdministrador
            ? await _context.Usuarios.CountAsync(u => u.Activo)
            : 0;

        var usuariosInactivos = esAdministrador
            ? await _context.Usuarios.CountAsync(u => !u.Activo)
            : 0;

        // Últimos cinco tickets dentro del alcance del rol.
        var ticketsRecientes = await consultaTickets
            .Include(t => t.Usuario)
            .Include(t => t.Categoria)
            .OrderByDescending(t => t.FechaCreacion)
            .Take(5)
            .Select(t => new
            {
                id = t.Id,
                titulo = t.Titulo,
                estado = t.Estado,
                prioridad = t.Prioridad,
                usuario = t.Usuario != null
                    ? t.Usuario.Nombre
                    : "Sin usuario",
                categoria = t.Categoria != null
                    ? t.Categoria.Nombre
                    : "Sin categoría",
                fechaCreacion = t.FechaCreacion
            })
            .ToListAsync();

        return Ok(new
        {
            totalTickets,
            ticketsAbiertos,
            ticketsEnProceso,
            ticketsResueltos,
            ticketsCerrados,
            ticketsAlta,
            ticketsMedia,
            ticketsBaja,
            ticketsPorCategoria,
            ticketsPorEstado,
            usuariosActivos,
            usuariosInactivos,
            ticketsRecientes
        });
    }
}
