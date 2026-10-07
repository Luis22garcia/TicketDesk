namespace TicketDesk.Api.DTOs;

public class TicketResponseDto
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public string Prioridad { get; set; } = string.Empty;

    public int UsuarioId { get; set; }

    public string UsuarioNombre { get; set; } = string.Empty;

    public int CategoriaId { get; set; }

    public string CategoriaNombre { get; set; } = string.Empty;
    public int? AgenteId { get; set; }

    public string AgenteNombre { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}