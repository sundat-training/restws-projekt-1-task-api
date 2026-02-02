using System.Security.Claims;
using Microsoft.Data.Sqlite;
using feature_5_auth.Data;
using feature_5_auth.Models;

namespace feature_5_auth.Middleware
{
    // TODO AUFGABE: Authentication Middleware implementieren
    // Was du tun musst:
    // 1. Lies den Authorization Header aus dem Request
    // 2. Prüfe ob der Header vorhanden ist (starts with "Bearer ")
    // 3. Extrahiere die userId aus dem Token (z.B. "Bearer user-1" -> "user-1")
    // 4. Optional: Prüfe ob der User in der Datenbank existiert
    // 5. Speichere die userId in HttpContext.Items["UserId"] für Controller
    // 6. Bei fehlendem/ungültigem Auth: return 401 Unauthorized
    //
    // Einfache Implementierung (ohne JWT):
    // - Token Format: "Bearer user-1" oder "Bearer user-2"
    // - Extrahiere einfach die ID nach "Bearer "
    // - Keine komplexe Token-Validierung nötig
    //
    // Tipp: Siehe HINTS.md für Code-Beispiele
    
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // PLATZHALTER: Aktuell wird jede Request durchgelassen
            // TODO: Authentication Logik hier implementieren
            
            await _next(context);
        }

        // Hilfsmethode: Prüfe ob ein Path öffentlich ist (kein Auth nötig)
        private bool IsPublicPath(string path)
        {
            // Login und Register sollten öffentlich sein
            return path.StartsWith("/api/auth/");
        }
    }

    // Extension Method für einfache Registrierung
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
