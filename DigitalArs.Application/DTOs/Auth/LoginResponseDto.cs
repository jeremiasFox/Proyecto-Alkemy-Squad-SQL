namespace DigitalArs.Application.DTOs.Auth;

// Contiene la información que la API devuelve después de un login exitoso.
public class LoginResponseDto
{
    // Token JWT que el cliente utilizará para autenticarse
    // en las siguientes peticiones.
    public string Token { get; set; } = string.Empty;

    // Fecha y hora en la que el token dejará de ser válido.
    public DateTime ExpiresAt { get; set; }
}