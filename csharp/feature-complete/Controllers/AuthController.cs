using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using feature_complete.Models;
using feature_complete.Services;
using feature_complete.Validators;

namespace feature_complete.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly RegisterRequestValidator _registerValidator;
        private readonly LoginRequestValidator _loginValidator;

        public AuthController(
            AuthService authService,
            RegisterRequestValidator registerValidator,
            LoginRequestValidator loginValidator)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            try
            {
                var validationResult = _registerValidator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return FormatValidationErrors(validationResult);
                }

                var result = _authService.Register(request.Username, request.Password);
                
                if (result == null)
                {
                    return Conflict(new { error = "Username already exists" });
                }

                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Registration failed", message = ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                var validationResult = _loginValidator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return FormatValidationErrors(validationResult);
                }

                var result = _authService.Authenticate(request.Username, request.Password);
                
                if (result == null)
                {
                    return Unauthorized(new { error = "Invalid credentials" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Login failed", message = ex.Message });
            }
        }

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Authentication required" });
                }

                var profile = _authService.GetProfile(userId);
                
                if (profile == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get profile", message = ex.Message });
            }
        }

        private string? GetCurrentUserId()
        {
            return HttpContext.Items["UserId"] as string;
        }

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
