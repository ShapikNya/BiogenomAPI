using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
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

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        object? result = null;

        switch (exception)
        {
         /*   case ValidationException validationException:
                code = HttpStatusCode.BadRequest;
                result = validationException.Errors
                    .Select(e => new
                    {
                        e.PropertyName,
                        e.AttemptedValue,
                        e.ErrorMessage
                    });
                break;*/

            default: 
                code = HttpStatusCode.InternalServerError;
                result = new { message = "Внутрення ошибка сервера" };
                break;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("========== Exception ==========");
        Console.WriteLine(exception.GetType().Name);
        Console.WriteLine(exception.Message);
        Console.WriteLine(exception.StackTrace);
        if (exception.InnerException != null)
        {
            Console.WriteLine("---- Inner Exception ----");
            Console.WriteLine(exception.InnerException.Message);
            Console.WriteLine(exception.InnerException.StackTrace);
        }
        Console.ResetColor();

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var json = JsonSerializer.Serialize(result);
        return context.Response.WriteAsync(json);
    }
}
