using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MunicipalPermitSystem.Api.Configuration;

namespace MunicipalPermitSystem.Api.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwt;
    private readonly TimeProvider _clock;

    public JwtTokenService(IOptions<JwtSettings> jwt, TimeProvider clock)
    {
        _jwt = jwt.Value;
        _clock = clock;
    }

    public (string Token, DateTimeOffset ExpiresAt) CreateToken(string subjectId, string name, string role)
    {
        var now = _clock.GetUtcNow();
        var expires = now.AddMinutes(_jwt.ExpiryMinutes);

        // Short claim names ("sub", "name", "role"). Program.cs tells the JWT middleware that
        // "name" and "role" are the name/role claims, so [Authorize(Roles = "...")] works.
        var claims = new[]
        {
            new Claim("sub", subjectId),
            new Claim("name", name),
            new Claim("role", role),
            new Claim("jti", Guid.NewGuid().ToString("N"))
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
