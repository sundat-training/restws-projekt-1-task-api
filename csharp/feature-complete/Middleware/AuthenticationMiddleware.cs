using feature_complete.Services;

namespace feature_complete.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;
        private readonly AuthService _authService;

        public AuthenticationMiddleware(
            RequestDelegate next, 
            ILogger<AuthenticationMiddleware> logger,
            AuthService authService)
        {
            _next = next;
            _logger = logger;
            _authService = authService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip authentication for public endpoints
            if (IsPublicPath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Get Authorization header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Authentication required" });
                return;
            }

            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid authorization format. Use 'Bearer token'" });
                return;
            }

            // Extract token
            var token = authHeader.Substring("Bearer ".Length).Trim();

            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid token" });
                return;
            }

            // Validate token and get userId
            var userId = _authService.ValidateToken(token);

            if (string.IsNullOrEmpty(userId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid or expired token" });
                return;
            }

            // Store userId in HttpContext for controllers
            context.Items["UserId"] = userId;

            await _next(context);
        }

        private bool IsPublicPath(string path)
        {
            // Public endpoints that don't require authentication
            var publicPaths = new[]
            {
                "/api/auth/login",
                "/api/auth/register"
            };

            return publicPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
