using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketDesk.Api.Data;
using TicketDesk.Api.DTOs;
using TicketDesk.Api.Models;

namespace TicketDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
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
    // ========================================

    [HttpGet]
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
    // GET: api/categorias/1
    // Solo Administrador
    // ========================================

    [HttpGet("{id}")]
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

    [HttpPut("{id}")]
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
    //
    // No eliminamos físicamente.
    // Desactivamos la categoría.
    // ========================================

    [HttpDelete("{id}")]
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