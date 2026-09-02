namespace DigitalArs.Application.DTOs.User;

public class UserFilterDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int? RoleId { get; set; }
    public bool? IsActive { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
