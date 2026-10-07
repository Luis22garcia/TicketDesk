namespace TicketDesk.Api.Models;

public class Ticket
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Estado { get; set; } = "Abierto";

    public string Prioridad { get; set; } = "Media";

    public int UsuarioId { get; set; }

    public int CategoriaId { get; set; }

    public int? AgenteId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    // Relación con Usuario
    public Usuario? Usuario { get; set; }

    // Relación con Categoria
    public Categoria? Categoria { get; set; }

    // Relación con Agente
    public Usuario? Agente { get; set; }
}