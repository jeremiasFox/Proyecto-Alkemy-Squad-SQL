using DigitalArs.Application.DTOs.User;
using DigitalArs.Application.Exceptions;
using DigitalArs.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalArs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET /api/users?name=&email=&roleId=&isActive=&pageNumber=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserFilterDto filter)
    {
        var result = await _userService.GetAllAsync(filter);
        return Ok(result);
    }

    // GET /api/users/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("Usuario", id);
        return Ok(user);
    }

    // POST /api/users
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserCreateRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            throw new ValidationException(errors);
        }

        var user = await _userService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    // PUT /api/users/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserUpdateRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            throw new ValidationException(errors);
        }

        var user = await _userService.UpdateAsync(id, dto);
        if (user is null)
            throw new NotFoundException("Usuario", id);
        return Ok(user);
    }

    // DELETE /api/users/{id}  → baja lógica (IsActive = false)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _userService.DeleteAsync(id);
        if (!deleted)
            throw new NotFoundException("Usuario", id);
        return NoContent();
    }
}
