using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FinanceTracking.API.Exceptions;

namespace FinanceTracking.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); 
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        context.Response.StatusCode = exception switch
        {
            BadRequestException => (int)HttpStatusCode.BadRequest, // 400
            ForbiddenException => (int)HttpStatusCode.Forbidden,   // 403
            NotFoundException => (int)HttpStatusCode.NotFound,     // 404
            ConflictException => (int)HttpStatusCode.Conflict,     // 409
            _ => (int)HttpStatusCode.InternalServerError           // 500
        };

        var responseMessage = exception.Message;

        if (context.Response.StatusCode == (int)HttpStatusCode.InternalServerError)
        {
            Console.WriteLine($"Unhandled exception: {exception}"); // Log the full exception details
            responseMessage = "An unexpected error occurred on the server."; 
        }

        var response = new { message = responseMessage };
        var jsonResponse = JsonSerializer.Serialize(response);
        
        return context.Response.WriteAsync(jsonResponse);
    }
}