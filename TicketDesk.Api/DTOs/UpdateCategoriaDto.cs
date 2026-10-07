using System.ComponentModel.DataAnnotations;

namespace TicketDesk.Api.DTOs;

public class UpdateCategoriaDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(
        100,
        ErrorMessage = "El nombre no puede superar los 100 caracteres."
    )]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(
        250,
        ErrorMessage = "La descripción no puede superar los 250 caracteres."
    )]
    public string Descripcion { get; set; } = string.Empty;

    public bool Activa { get; set; } = true;
}