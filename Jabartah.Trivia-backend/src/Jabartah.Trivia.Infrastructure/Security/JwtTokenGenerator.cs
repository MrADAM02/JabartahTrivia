using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Jabartah.Trivia.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Jabartah.Trivia.Infrastructure.Security;

public class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
{
    public string Generate(Guid userId, string email, string name)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Missing 'Jwt:Key'.");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Missing 'Jwt:Issuer'.");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Missing 'Jwt:Audience'.");
        var expiryMinutes = int.Parse(jwtSection["ExpiryMinutes"] ?? "10080");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
