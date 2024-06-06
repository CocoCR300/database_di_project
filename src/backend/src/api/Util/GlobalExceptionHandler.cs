using Microsoft.AspNetCore.Diagnostics;

namespace Restify.API.Util;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, 
        Exception exception, 
        CancellationToken cancellation)
    {
        var error = new { exception.Message };
        await context.Response.WriteAsJsonAsync(error, cancellation);
        return true;
    }
}