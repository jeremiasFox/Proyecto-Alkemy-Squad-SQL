using System.ComponentModel.DataAnnotations;

namespace DigitalArs.Application.DTOs.User;

public class UserUpdateMeRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}
