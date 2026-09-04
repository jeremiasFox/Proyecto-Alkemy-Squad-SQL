namespace DigitalArs.Application.Exceptions;

// Se utiliza cuando los datos recibidos no cumplen con las validaciones esperadas.
public class ValidationException : AppException
{
    // Permite devolver varios errores de validación en una misma respuesta.
    public ValidationException(IEnumerable<string> errors)
        : base("La solicitud contiene datos inválidos.", 400, errors) { }

    // Atajo para cuando solo necesitamos informar un error de validación.
    public ValidationException(string error)
        : base("La solicitud contiene datos inválidos.", 400, [error]) { }
}