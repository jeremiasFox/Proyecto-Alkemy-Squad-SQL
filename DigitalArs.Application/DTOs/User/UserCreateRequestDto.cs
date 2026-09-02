using System.ComponentModel.DataAnnotations;

namespace DigitalArs.Application.DTOs.User;

public class UserCreateRequestDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Email no válido")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Mínimo 8 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required]
    public int RoleId { get; set; }
}

