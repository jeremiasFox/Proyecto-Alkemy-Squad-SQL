using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DigitalArs.Application.Common.Interfaces;
using DigitalArs.Application.DTOs.Auth;
using DigitalArs.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DigitalArs.Infrastructure.Security;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LoginResponseDto GenerateToken(User user)
    {
        // Obtenemos la configuración necesaria para crear y firmar el JWT.
        // La SecretKey es obligatoria porque se utiliza para validar que
        // el token fue generado por nuestra aplicación.
        var secretKey = _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey no está configurada.");

        var issuer      = _configuration["JwtSettings:Issuer"]!;
        var audience    = _configuration["JwtSettings:Audience"]!;
        var expMinutes  = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");

        // Creamos la clave y las credenciales que se utilizarán para firmar
        // el token. HmacSha256 es el algoritmo utilizado para la firma.
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Definimos hasta cuándo será válido el token.
        var expiresAt   = DateTime.UtcNow.AddMinutes(expMinutes);

        // Los claims contienen información que viaja dentro del JWT.
        // Se utilizan posteriormente para identificar y autorizar al usuario.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role,               user.Role?.Name ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        // Creamos el token utilizando la configuración, los claims,
        // la fecha de expiración y las credenciales de firma.
        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            expiresAt,
            signingCredentials: credentials);

        // Convertimos el token a un string para poder enviarlo al cliente
        // junto con la fecha en la que dejará de ser válido.
        return new LoginResponseDto
        {
            Token     = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }
}