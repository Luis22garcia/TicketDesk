using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketDesk.Api.Data;

namespace TicketDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Agente")]
public class DashboardController : ControllerBase
{
    private readonly TicketDeskDbContext _context;

    public DashboardController(TicketDeskDbContext context)
    {
        _context = context;
    }

    // ========================================
    // GET: api/dashboard
    //
    // Administrador y Agente
    // ========================================

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var totalTickets = await _context.Tickets.CountAsync();

        var ticketsAbiertos = await _context.Tickets
            .CountAsync(t => t.Estado == "Abierto");

        var ticketsEnProceso = await _context.Tickets
            .CountAsync(t => t.Estado == "En proceso");

        var ticketsResueltos = await _context.Tickets
            .CountAsync(t => t.Estado == "Resuelto");

        var ticketsCerrados = await _context.Tickets
            .CountAsync(t => t.Estado == "Cerrado");

        var ticketsAlta = await _context.Tickets
            .CountAsync(t => t.Prioridad == "Alta");

        var ticketsMedia = await _context.Tickets
            .CountAsync(t => t.Prioridad == "Media");

        var ticketsBaja = await _context.Tickets
            .CountAsync(t => t.Prioridad == "Baja");

        var ticketsPorCategoria = await _context.Tickets
            .Include(t => t.Categoria)
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

        var ticketsPorEstado = await _context.Tickets
            .GroupBy(t => t.Estado)
            .Select(g => new
            {
                estado = g.Key,
                cantidad = g.Count()
            })
            .OrderByDescending(x => x.cantidad)
            .ToListAsync();

        var usuariosActivos = await _context.Usuarios
             .CountAsync(u => u.Activo);

        var usuariosInactivos = await _context.Usuarios
             .CountAsync(u => !u.Activo);

        var ticketsRecientes = await _context.Tickets
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