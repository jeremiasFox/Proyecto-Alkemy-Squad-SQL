using DigitalArs.Application.DTOs.Auth;

using DigitalArs.Domain.Entities;

namespace DigitalArs.Application.Common.Interfaces;

// Define el contrato para el servicio encargado de generar los tokens JWT.
public interface IJwtService
{
    // Genera un token a partir de la información del usuario autenticado.
    LoginResponseDto GenerateToken(User user);
}