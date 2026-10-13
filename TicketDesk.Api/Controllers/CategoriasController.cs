
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketDesk.Api.Data;
using TicketDesk.Api.DTOs;
using TicketDesk.Api.Models;

namespace TicketDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly TicketDeskDbContext _context;

    public CategoriasController(TicketDeskDbContext context)
    {
        _context = context;
    }

    // ========================================
    // GET: api/categorias
    // Solo Administrador
    // Consulta todas las categorías,
    // incluidas las desactivadas.
    // ========================================

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetCategorias()
    {
        var categorias = await _context.Categorias
            .Select(c => new
            {
                c.Id,
                c.Nombre,
                c.Descripcion,
                c.Activa
            })
            .ToListAsync();

        return Ok(categorias);
    }

    // ========================================
    // GET: api/categorias/activas
    // Administrador, Agente y Usuario
    // Solo devuelve categorías activas.
    // ========================================

    [HttpGet("activas")]
    [Authorize(Roles = "Administrador,Agente,Usuario")]
    public async Task<IActionResult> GetCategoriasActivas()
    {
        var categorias = await _context.Categorias
            .Where(c => c.Activa)
            .Select(c => new
            {
                c.Id,
                c.Nombre,
                c.Descripcion,
                c.Activa
            })
            .ToListAsync();

        return Ok(categorias);
    }

    // ========================================
    // GET: api/categorias/1
    // Solo Administrador
    // ========================================

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> GetCategoria(int id)
    {
        var categoria = await _context.Categorias
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Nombre,
                c.Descripcion,
                c.Activa
            })
            .FirstOrDefaultAsync();

        if (categoria == null)
        {
            return NotFound(new
            {
                mensaje = "La categoría no existe."
            });
        }

        return Ok(categoria);
    }

    // ========================================
    // POST: api/categorias
    // Solo Administrador
    // ========================================

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CreateCategoria(
        CreateCategoriaDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var nombreExiste = await _context.Categorias
            .AnyAsync(c => c.Nombre == dto.Nombre);

        if (nombreExiste)
        {
            return Conflict(new
            {
                mensaje = "Ya existe una categoría con ese nombre."
            });
        }

        var categoria = new Categoria
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Activa = true
        };

        _context.Categorias.Add(categoria);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategoria),
            new { id = categoria.Id },
            new
            {
                categoria.Id,
                categoria.Nombre,
                categoria.Descripcion,
                categoria.Activa
            }
        );
    }

    // ========================================
    // PUT: api/categorias/1
    // Solo Administrador
    // ========================================

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateCategoria(
        int id,
        UpdateCategoriaDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var categoria = await _context.Categorias
            .FindAsync(id);

        if (categoria == null)
        {
            return NotFound(new
            {
                mensaje = "La categoría no existe."
            });
        }

        var nombreExiste = await _context.Categorias
            .AnyAsync(c =>
                c.Nombre == dto.Nombre &&
                c.Id != id);

        if (nombreExiste)
        {
            return Conflict(new
            {
                mensaje = "Ya existe otra categoría con ese nombre."
            });
        }

        categoria.Nombre = dto.Nombre;
        categoria.Descripcion = dto.Descripcion;
        categoria.Activa = dto.Activa;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ========================================
    // DELETE: api/categorias/1
    // Solo Administrador
    // Desactiva la categoría sin eliminarla.
    // ========================================

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteCategoria(int id)
    {
        var categoria = await _context.Categorias
            .FindAsync(id);

        if (categoria == null)
        {
            return NotFound(new
            {
                mensaje = "La categoría no existe."
            });
        }

        categoria.Activa = false;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
