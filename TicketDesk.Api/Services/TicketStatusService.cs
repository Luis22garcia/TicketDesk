namespace TicketDesk.Api.Services;

public class TicketStatusService
{
    public bool EsTransicionValida(string estadoActual, string nuevoEstado)
    {
        if (estadoActual == nuevoEstado)
        {
            return true;
        }

        return estadoActual switch
        {
            "Abierto" => nuevoEstado == "En proceso",
            "En proceso" => nuevoEstado == "Resuelto",
            "Resuelto" => nuevoEstado == "Cerrado",
            "Cerrado" => false,
            _ => false
        };
    }
}