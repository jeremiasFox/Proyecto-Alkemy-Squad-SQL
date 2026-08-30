namespace DigitalArs.Application.Exceptions;

// Se utiliza cuando el usuario no está autenticado y debe iniciar sesión.
public class UnauthorizedException : AppException
{
    public UnauthorizedException()
        : base("No está autenticado. Por favor inicie sesión.", 401) { }

    // Permite utilizar un mensaje personalizado manteniendo el código 401.
    public UnauthorizedException(string message)
        : base(message, 401) { }
}