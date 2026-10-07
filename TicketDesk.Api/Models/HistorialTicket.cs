namespace TicketDesk.Api.Models;

public class HistorialTicket
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public int UsuarioId { get; set; }

    public string Accion { get; set; } = string.Empty;

    public string? EstadoAnterior { get; set; }

    public string? EstadoNuevo { get; set; }

    public string? Comentario { get; set; }

    public DateTime Fecha { get; set; }

    // Relación con Ticket
    public Ticket? Ticket { get; set; }

    // Usuario que realizó la acción
    public Usuario? Usuario { get; set; }
}