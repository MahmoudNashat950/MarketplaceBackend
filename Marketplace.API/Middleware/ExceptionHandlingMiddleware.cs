using System.Net;
using System.Text.Json;

namespace MarketplaceBackend.Middleware;

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
            Console.WriteLine(" ERROR: " + ex.Message);
            Console.WriteLine(ex.StackTrace);

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                message = ex.Message
            });
        }
    }
}
