using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
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
        /// Аутентификация пользователя и получение JWT-токена
        /// </summary>
        /// <param name="loginDto">Данные для входа (email и пароль)</param>
        /// <returns>JWT-токен</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // 1. Валидация модели
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 2. Проверка учетных данных (заглушка - в реальном проекте используйте Identity или свою БД)
                if (loginDto.Email != "admin@example.com" || loginDto.Password != "1234567890")
                    return Unauthorized(new { Message = "Неверный email или пароль" });

                // 3. Генерация токена
                var token = GenerateJwtToken(loginDto.Email);

                // 4. Возврат токена
                return Ok(new
                {
                    Token = token,
                    ExpiresIn = 3600 // Время жизни токена в секундах
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка сервера: " + ex.Message });
            }
        }

        private string GenerateJwtToken(string email)
        {
            // 1. Получаем ключ из конфигурации
            var secretKey = _config["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key не настроен");
            var issuer = _config["Jwt:Issuer"] ?? "OnlineStore";
            var audience = _config["Jwt:Audience"] ?? "OnlineStoreClients";

            // 2. Создаем криптографический ключ
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Формируем claims (данные пользователя)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.NameIdentifier, email),
                new Claim(ClaimTypes.Role, "Admin"), // Роль
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Уникальный идентификатор токена
            };

            // 4. Создаем токен
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            // 5. Сериализуем токен в строку
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    /// <summary>
    /// DTO для входа в систему
    /// </summary>
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}