using AutoMapper;
using DigitalArs.Application.Common.Interfaces;
using DigitalArs.Application.DTOs.User;
using DigitalArs.API.Helpers;
using DigitalArs.Domain.Entities;
using DigitalArs.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalArs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;

    public UsersController(
        AppDbContext context,
        IMapper mapper,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserCreateRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
              .SelectMany(v => v.Errors)
              .Select(e => e.ErrorMessage)
              .ToList();
            return BadRequest(new { Errors = errors });
        }

        var roleExists = await _context.Roles.AnyAsync(r => r.Id == dto.RoleId);
        if (!roleExists)
            return BadRequest(new { Errors = new[] { $"El RoleId {dto.RoleId} no existe" } });

        var user = _mapper.Map<User>(dto);
        user.Password = _passwordHasher.Hash(dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<UserResponseDto>(user);
        return CreatedAtAction(nameof(Create), new { id = user.Id }, response);
    }
}