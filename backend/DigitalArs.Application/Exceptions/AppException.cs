namespace DigitalArs.Application.Exceptions;

public class AppException : Exception
{
    // Código HTTP que se devolverá cuando esta excepción sea manejada.
    public int StatusCode { get; }

    // Permite devolver uno o varios errores relacionados con la excepción.
    public IReadOnlyList<string> Errors { get; }

    public AppException(
        string message,
        int statusCode = 400,
        IEnumerable<string>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;

        // Convertimos los errores a una lista de solo lectura para evitar
        // que puedan modificarse después de crear la excepción.
        Errors = errors?.ToList().AsReadOnly() ?? [];
    }
}