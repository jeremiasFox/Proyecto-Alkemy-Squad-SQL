using DigitalArs.Application.Common.DTOs;
using DigitalArs.Application.Common.DTOs.Interfaces;
using DigitalArs.Application.Common.Interfaces;
using DigitalArs.Application.DTOs.User;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalArs.Application.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IAppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<PaginatedResponseDto<UserResponseDto>> GetAllAsync(UserFilterDto filter)
    {
        var query = _context.Users.Include(u => u.Role).AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.Name)) query = query.Where(u => u.Name.Contains(filter.Name));
        if (!string.IsNullOrWhiteSpace(filter.Email)) query = query.Where(u => u.Email.Contains(filter.Email));
        if (filter.RoleId.HasValue) query = query.Where(u => u.RoleId == filter.RoleId);
        if (filter.IsActive.HasValue) query = query.Where(u => u.IsActive == filter.IsActive.Value);

        var total = await query.CountAsync();
        var items = await query.OrderBy(u => u.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                RoleId = u.RoleId,
                RoleName = u.Role.Name,
                IsActive = u.IsActive
            }).ToListAsync();

        return new PaginatedResponseDto<UserResponseDto> { Items = items, TotalCount = total, PageNumber = filter.PageNumber, PageSize = filter.PageSize };
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Users.Include(u => u.Role).Where(u => u.Id == id)
            .Select(u => new UserResponseDto { Id = u.Id, Name = u.Name, Email = u.Email, RoleId = u.RoleId, RoleName = u.Role.Name, IsActive = u.IsActive }).FirstOrDefaultAsync();
    }

    public async Task<UserResponseDto> CreateAsync(UserCreateRequestDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email)) throw new InvalidOperationException("EMAIL_EXISTS");
        var user = new User { Name = dto.Name, Email = dto.Email, Password = _passwordHasher.Hash(dto.Password), RoleId = dto.RoleId, IsActive = true };
        _context.Users.Add(user); await _context.SaveChangesAsync(); return (await GetByIdAsync(user.Id))!;
    }

    public async Task<UserResponseDto?> UpdateAsync(int id, UserUpdateRequestDto dto)
    {
        var user = await _context.Users.FindAsync(new object[] { id });
        if (user == null) return null;
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id)) throw new InvalidOperationException("EMAIL_EXISTS");
        user.Name = dto.Name; user.Email = dto.Email; user.RoleId = dto.RoleId; await _context.SaveChangesAsync(); return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(new object[] { id });
        if (user == null) return false;
        user.IsActive = false; await _context.SaveChangesAsync(); return true;
    }
}