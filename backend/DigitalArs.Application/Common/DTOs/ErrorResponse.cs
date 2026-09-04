namespace DigitalArs.Application.Common.DTOs;

public sealed class ErrorResponse
{
    // Código HTTP que se devuelve al cliente.
    public int StatusCode { get; init; }

    // Mensaje principal del error.
    public string Message { get; init; } = string.Empty;

    // Lista de errores adicionales, si existen.
    public IReadOnlyList<string> Errors { get; init; } = [];

    // Identificador que permite encontrar el error en los logs.
    public string TraceId { get; init; } = string.Empty;
}