using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace OnlineStore.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Выполняет аутентификацию пользователя по email и паролю и возвращает JWT-токен при успешном входе.
        /// </summary>
        /// <param name="loginDto">Модель с email и паролем пользователя</param>
        /// <returns>JWT-токен и время его жизни в секундах, либо сообщение об ошибке</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var validationResult = ValidateLoginModel(loginDto);
                if (validationResult != null)
                    return validationResult;

                if (!CheckCredentials(loginDto))
                    return Unauthorized(new { Message = "Неверный email или пароль" });

                var token = GenerateJwtToken(loginDto.Email);

                return Ok(new
                {
                    Token = token,
                    ExpiresIn = 3600
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ошибка сервера: {ex.Message}" });
            }
        }

        private IActionResult? ValidateLoginModel(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                return BadRequest(new { Message = "Email и пароль обязательны для заполнения" });

            return null;
        }

        private bool CheckCredentials(LoginDto loginDto)
        {
            return loginDto.Email == "admin@example.com" && loginDto.Password == "1234567890";
        }

        private string GenerateJwtToken(string email)
        {
            var secretKey = _config["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key не настроен");
            var issuer = _config["Jwt:Issuer"] ?? "OnlineStore";
            var audience = _config["Jwt:Audience"] ?? "OnlineStoreClients";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.NameIdentifier, email),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    /// <summary>
    /// Модель для передачи данных при входе пользователя (email и пароль)
    /// </summary>
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}