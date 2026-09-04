using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DigitalArs.API.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(value, out var id))
            return id;

        throw new InvalidOperationException("El token no contiene un userId válido.");
    }

    public static string GetEmail(this ClaimsPrincipal user)
        => user.FindFirstValue(JwtRegisteredClaimNames.Email)
           ?? user.FindFirstValue(ClaimTypes.Email)
           ?? string.Empty;

    public static string GetRole(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
}
