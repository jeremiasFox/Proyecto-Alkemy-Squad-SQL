using DigitalArs.Application.Common.Interfaces;

using DigitalArs.Application.DTOs.Auth;

using DigitalArs.Application.Exceptions;

using DigitalArs.Infrastructure.Data;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

namespace DigitalArs.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthController(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _context        = context;
        _passwordHasher = passwordHasher;
        _jwtService     = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        // Verificamos que los datos recibidos cumplan con las validaciones
        // definidas en el DTO antes de consultar la base de datos.
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);

            throw new ValidationException(errors);
        }

        // Buscamos al usuario por email y cargamos su Role porque
        // necesitamos esa información para generar el token JWT.
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        // Usamos el mismo mensaje tanto si el usuario no existe como si
        // la contraseña es incorrecta, evitando revelar información
        // sobre qué emails están registrados.
        if (user is null || !_passwordHasher.Verify(dto.Password, user.Password))
            throw new UnauthorizedException("Credenciales inválidas.");

        // Aunque las credenciales sean correctas, no permitimos iniciar
        // sesión si la cuenta está desactivada.
        if (!user.IsActive)
            throw new UnauthorizedException("La cuenta se encuentra desactivada.");

        // Generamos el JWT con la información necesaria del usuario
        // y lo devolvemos al cliente para futuras peticiones autenticadas.
        return Ok(_jwtService.GenerateToken(user));
    }
}