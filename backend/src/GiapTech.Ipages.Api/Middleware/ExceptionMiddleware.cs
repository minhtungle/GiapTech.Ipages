using System.Net;
using System.Text.Json;
using GiapTech.Ipages.Application.Common.Exceptions;

namespace GiapTech.Ipages.Api;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var response = ex switch
        {
            ValidationException ve => new ErrorResponse(
                (int)HttpStatusCode.UnprocessableEntity,
                "Validation failed",
                ve.Errors),
            NotFoundException nfe => new ErrorResponse(
                (int)HttpStatusCode.NotFound,
                nfe.Message),
            UnauthorizedException ue => new ErrorResponse(
                (int)HttpStatusCode.Unauthorized,
                ue.Message),
            ForbiddenException fe => new ErrorResponse(
                (int)HttpStatusCode.Forbidden,
                fe.Message),
            _ => new ErrorResponse(
                (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred.")
        };

        context.Response.StatusCode = response.StatusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}

public record ErrorResponse(int StatusCode, string Message, object? Errors = null);
