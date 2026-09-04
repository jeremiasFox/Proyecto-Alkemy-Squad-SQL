using System.ComponentModel.DataAnnotations;
namespace DigitalArs.Application.DTOs.User;

public class UserUpdateRequestDto
{
    [Required][MaxLength(100)] public string Name { get; set; } = "";
    [Required][EmailAddress] public string Email { get; set; } = "";
    [Required] public int RoleId { get; set; }
}
