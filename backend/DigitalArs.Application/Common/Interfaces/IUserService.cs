using DigitalArs.Application.Common.DTOs;
using DigitalArs.Application.DTOs.User;
namespace DigitalArs.Application.Interfaces;

public interface IUserService
{
    Task<PaginatedResponseDto<UserResponseDto>> GetAllAsync(UserFilterDto filter);
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task<UserResponseDto> CreateAsync(UserCreateRequestDto dto);
    Task<UserResponseDto?> UpdateAsync(int id, UserUpdateRequestDto dto);
    Task<bool> DeleteAsync(int id);
}