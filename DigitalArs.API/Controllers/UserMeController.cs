using AutoMapper;
using DigitalArs.Application.DTOs.User;
using DigitalArs.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DigitalArs.Application.Common.Interfaces;

namespace DigitalArs.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize] // Cualquier usuario logueado, no solo Admin
public class UserMeController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;

    public UserMeController(AppDbContext context, IMapper mapper, IPasswordHasher passwordHasher)
    {
        _context = context;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    private int GetUserIdFromToken()
    {
        // En algunos proyectos viene como NameIdentifier, en otros como "uid" o "sub"
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("uid")
                ?? User.FindFirstValue(ClaimTypes.Name);
        return int.Parse(claim!);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserIdFromToken();

        var user = await _context.Users
           .Include(u => u.Role)
           .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound();

        var response = _mapper.Map<UserResponseDto>(user);
        // Si tu mapper no mapea RoleName, lo seteamos manual
        response.RoleName = user.Role?.Name ?? "";

        return Ok(response);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UserUpdateMeRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserIdFromToken();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        // 1. Actualiza solo Nombre -> cumple "No puede cambiar rol ni saldo"
        user.Name = dto.Name;

        // 2. Lógica de contraseña: si quiere cambiar, debe mandar la actual
        if (!string.IsNullOrEmpty(dto.NewPassword))
        {
            if (string.IsNullOrEmpty(dto.CurrentPassword))
                return BadRequest(new { Errors = new[] { "Debe enviar la contraseña actual para cambiarla" } });

            if (!_passwordHasher.Verify(dto.CurrentPassword, user.Password))
                return BadRequest(new { Errors = new[] { "La contraseña actual es incorrecta" } });

            user.Password = _passwordHasher.Hash(dto.NewPassword);
        }

        await _context.SaveChangesAsync();

        var userUpdated = await _context.Users.Include(u => u.Role).FirstAsync(u => u.Id == userId);
        var response = _mapper.Map<UserResponseDto>(userUpdated);
        response.RoleName = userUpdated.Role?.Name ?? "";

        return Ok(response);
    }
}