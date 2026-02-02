using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Microsoft.Data.Sqlite;
using feature_5_auth.Data;
using feature_5_auth.Models;
using feature_5_auth.Validators;

namespace feature_5_auth.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseConfig _database;
        private readonly LoginRequestValidator _loginValidator;

        public AuthController()
        {
            _database = new DatabaseConfig();
            _loginValidator = new LoginRequestValidator();
        }

        // ============================================================
        // TODO AUFGABE: POST /api/auth/login implementieren
        // ============================================================
        // Was du tun musst:
        // 1. Validiere den LoginRequest (Username und Password)
        // 2. Suche den User in der Datenbank anhand des Username
        // 3. Prüfe ob das Password übereinstimmt
        // 4. Bei Erfolg: return 200 OK mit { userId, username }
        // 5. Bei Fehler: return 401 Unauthorized mit { error: "Invalid credentials" }
        //
        // Einfache Variante (ohne Password Hashing):
        // - Klartext-Vergleich: user.Password == request.Password
        //
        // Bonus (mit Password Hashing):
        // - Verwende BCrypt für Password-Hashing
        // - NuGet: BCrypt.Net-Next
        //
        // Tipp: Siehe HINTS.md für Code-Beispiele
        // ============================================================
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                // TODO: Validierung durchführen
                // TODO: User in DB suchen
                // TODO: Password prüfen
                // TODO: Bei Erfolg: LoginResponse zurückgeben
                // TODO: Bei Fehler: 401 Unauthorized

                // PLATZHALTER: Aktuell wird 501 zurückgegeben
                return StatusCode(501, new { error = "Not implemented yet - implement Login here" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Login failed", message = ex.Message });
            }
        }

        // Hilfsmethode für Validierungsfehler-Formatierung
        private IActionResult FormatValidationErrors(FluentValidation.Results.ValidationResult validationResult)
        {
            var errors = validationResult.Errors.Select(e => new
            {
                field = e.PropertyName.ToLower(),
                message = e.ErrorMessage
            }).ToList();

            return BadRequest(new { errors });
        }
    }
}
