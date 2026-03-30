using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Settings;
using ManageAccountWebAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ManageAccountWebAPI.Services.Implementations
{
    /// <summary>
    /// Issues JWT access tokens.
    /// Registered as Singleton – depends only on IOptions&lt;JwtSettings&gt; which is also singleton-safe.
    /// </summary>
    public sealed class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly SymmetricSecurityKey _signingKey;

        public TokenService(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;

            if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
                throw new InvalidOperationException(
                    "JwtSettings:SecretKey is not configured. Check appsettings.json.");

            _signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        }

        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,        user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email,      user.Email),
                new Claim("UserId",                           user.Id.ToString())
            };

            var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer:             _jwtSettings.Issuer,
                audience:           _jwtSettings.Audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}