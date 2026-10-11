using EnterpriseIntegration.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EnterpriseIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly byte[] _expectedSecretHash;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            var expectedSecretHash = _configuration["PartnerAuth:ClientSecretHash"]
                ?? throw new InvalidOperationException("PartnerAuth:ClientSecretHash is not configured.");
            _expectedSecretHash = Convert.FromHexString(expectedSecretHash);
            if (_expectedSecretHash.Length != 32)
                throw new InvalidOperationException("PartnerAuth:ClientSecretHash must be a SHA-256 hash in hexadecimal format.");
        }

        [HttpPost("token")]
        public IActionResult GenerateToken([FromBody] LoginRequestDto request)
        {
            var suppliedHash = SHA256.HashData( Encoding.UTF8.GetBytes(request.ClientSecret ?? ""));

            var validSecret = CryptographicOperations.FixedTimeEquals(
                suppliedHash,
                _expectedSecretHash);

            if (request.ClientId != "AriaSystem" || !validSecret)
            {
                return Unauthorized(new { message = "شناسه کلاینت یا رمز عبور اشتباه است." });
            }

            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"]!;
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = Convert.ToInt32(jwtSettings["ExpiryMinutes"] ?? "1440"); 


            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, request.ClientId),
                new Claim(ClaimTypes.Role, "PartnerApiUser"),
                new Claim("Scope", "Invoice.Read")
            }),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new AuthResponseDto
            {
                AccessToken = tokenString,
                ExpiresInSeconds = expiryMinutes * 60
            });
        }
    }
}
