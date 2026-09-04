using DigitalArs.Application.Common.DTOs;
using DigitalArs.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace DigitalArs.API.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Usamos el TraceId para poder relacionar la respuesta con
        // el error registrado en los logs.
        var traceId = httpContext.TraceIdentifier;

        int statusCode;
        string message;
        IReadOnlyList<string> errors;

        // Las excepciones propias de la aplicación ya contienen
        // el código HTTP y los mensajes que queremos devolver.
        if (exception is AppException appException)
        {
            statusCode = appException.StatusCode;
            message = appException.Message;
            errors = appException.Errors;

            _logger.LogWarning(
                exception,
                "Error de aplicación [{StatusCode}] en {Path} — TraceId: {TraceId}",
                statusCode, httpContext.Request.Path, traceId);
        }
        else
        {
            // Cualquier excepción que no hayamos controlado se considera
            // un error interno del servidor.
            statusCode = StatusCodes.Status500InternalServerError;
            errors = [];

            // En producción no mostramos detalles internos del error.
            // En desarrollo mostramos el mensaje para facilitar el debugging.
            message = _environment.IsProduction()
                ? "Ocurrió un error interno. Por favor intente nuevamente más tarde."
                : exception.Message;

            _logger.LogError(
                exception,
                "Excepción no controlada en {Method} {Path} — TraceId: {TraceId}",
                httpContext.Request.Method, httpContext.Request.Path, traceId);
        }

        // Construimos una respuesta común para todos los errores de la API.
        var response = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Errors = errors,
            TraceId = traceId
        };

        // Configuramos la respuesta HTTP y la devolvemos como JSON.
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            cancellationToken);

        // true indica que la excepción ya fue manejada.
        return true;
    }
}