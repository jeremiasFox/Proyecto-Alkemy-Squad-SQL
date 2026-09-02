using System.ComponentModel.DataAnnotations;

namespace DigitalArs.Application.DTOs.Auth;

// Contiene los datos que el cliente debe enviar para iniciar sesión.
public class LoginRequestDto
{
    // El email es obligatorio y debe tener un formato válido.
    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public string Email { get; set; } = string.Empty;

    // La contraseña es obligatoria para poder iniciar sesión.
    [Required(ErrorMessage = "La contraseña es requerida.")]
    public string Password { get; set; } = string.Empty;
}