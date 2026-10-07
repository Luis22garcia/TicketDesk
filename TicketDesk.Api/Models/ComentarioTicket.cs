namespace TicketDesk.Api.Models;

public class ComentarioTicket
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public int UsuarioId { get; set; }

    public string Contenido { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    public Ticket? Ticket { get; set; }

    public Usuario? Usuario { get; set; }
}