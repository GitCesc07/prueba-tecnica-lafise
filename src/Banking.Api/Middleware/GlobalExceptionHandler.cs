using Banking.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title, code, detail) = exception switch
        {
            NotFoundException e => (StatusCodes.Status404NotFound, "Recurso no encontrado", e.Code, e.Message),
            BusinessException e => (StatusCodes.Status400BadRequest, "Regla de negocio incumplida", e.Code, e.Message),
            _ => (StatusCodes.Status500InternalServerError, "Error interno", "internal_error",
                  "Ocurrió un error inesperado. Intente nuevamente más tarde.")
        };

        if (status >= 500)
            logger.LogError(exception, "Error no controlado en {Path}", context.Request.Path);
        else
            logger.LogWarning("Error de negocio [{Code}] en {Path}: {Message}", code, context.Request.Path, exception.Message);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Extensions =
            {
                ["code"] = code,
                ["traceId"] = context.TraceIdentifier
            }
        };

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: ct);
        return true;
    }
}
