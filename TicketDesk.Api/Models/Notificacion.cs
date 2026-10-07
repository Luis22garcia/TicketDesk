namespace TicketDesk.Api.Models;

public class Notificacion
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int? TicketId { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public bool Leida { get; set; } = false;

    public DateTime Fecha { get; set; }

    public Usuario? Usuario { get; set; }

    public Ticket? Ticket { get; set; }
}