namespace DigitalArs.Application.Exceptions;

// Se utiliza cuando no se encuentra el recurso solicitado.
public class NotFoundException : AppException
{
    // Genera un mensaje indicando qué recurso no se encontró y su Id.
    public NotFoundException(string resourceName, object id)
        : base($"No se encontró {resourceName} con Id {id}.", 404) { }

    // Permite utilizar un mensaje personalizado manteniendo el código 404.
    public NotFoundException(string message)
        : base(message, 404) { }
}