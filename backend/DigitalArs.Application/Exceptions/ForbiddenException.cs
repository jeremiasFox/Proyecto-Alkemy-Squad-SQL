namespace DigitalArs.Application.Exceptions;

// Excepción utilizada cuando el usuario está autenticado,
// pero no tiene permisos para realizar una determinada operación.
public class ForbiddenException : AppException
{
    public ForbiddenException()
        : base("No tiene permisos para realizar esta operación.", 403) { }

    // Permite indicar un mensaje personalizado manteniendo el código 403.
    public ForbiddenException(string message)
        : base(message, 403) { }
}