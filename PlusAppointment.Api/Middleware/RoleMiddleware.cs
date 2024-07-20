using System.Security.Claims;

namespace WebApplication1.Middleware;

public class RoleMiddleware
{
    private readonly RequestDelegate _next;

    public RoleMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tokenType = context.Request.Headers["Token-Type"].FirstOrDefault();

        if (tokenType == "Access")
        {
            var userRole = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userRole))
            {
                context.Items["UserRole"] = userRole;
            }
        }

        await _next(context);
    }
}

public static class RoleMiddlewareExtensions
{
    public static IApplicationBuilder UseRoleMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RoleMiddleware>();
    }
}