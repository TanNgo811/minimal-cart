using System.Net;
using System.Text.Json;

namespace ECommerce.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            Status = "Error",
            Message = exception switch
            {
                UnauthorizedAccessException _ => "Unauthorized access",
                ArgumentException _ => "Invalid argument provided",
                KeyNotFoundException _ => "Requested resource not found",
                _ => "An internal server error occurred"
            },
            DetailedMessage = exception.Message
        };

        context.Response.StatusCode = exception switch
        {
            UnauthorizedAccessException _ => (int)HttpStatusCode.Unauthorized,
            ArgumentException _ => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException _ => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
