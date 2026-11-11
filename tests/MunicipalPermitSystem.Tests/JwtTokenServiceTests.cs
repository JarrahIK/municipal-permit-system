using Xunit;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MunicipalPermitSystem.Api.Configuration;
using MunicipalPermitSystem.Api.Services;
using MunicipalPermitSystem.Tests.Fakes;

namespace MunicipalPermitSystem.Tests;

public class JwtTokenServiceTests
{
    private static readonly string Key = new('k', 48);
    private readonly FakeClock _clock = new();

    private JwtTokenService Make() =>
        new(Options.Create(new JwtSettings { Key = Key, ExpiryMinutes = 60 }), _clock);

    private static string Claim(JwtSecurityToken token, string type) =>
        token.Claims.First(c => c.Type == type).Value;

    [Fact]
    public void Token_carries_subject_name_and_role()
    {
        var (token, _) = Make().CreateToken("SURV001", "Demo Surveyor", "Surveyor");
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("SURV001", Claim(parsed, "sub"));
        Assert.Equal("Demo Surveyor", Claim(parsed, "name"));
        Assert.Equal("Surveyor", Claim(parsed, "role"));
    }

    [Fact]
    public void Token_expires_after_the_configured_minutes()
    {
        var (token, expiresAt) = Make().CreateToken("C1", "Citizen", "Citizen");
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(_clock.Now.AddMinutes(60), expiresAt);
        Assert.Equal(expiresAt.ToUnixTimeSeconds(), new DateTimeOffset(parsed.ValidTo, TimeSpan.Zero).ToUnixTimeSeconds());
    }

    [Fact]
    public void Token_is_only_valid_with_the_signing_key_that_made_it()
    {
        // Validate against the real wall clock: use a token made "now".
        var service = new JwtTokenService(Options.Create(new JwtSettings { Key = Key }), TimeProvider.System);
        var (token, _) = service.CreateToken("C1", "Citizen", "Citizen");

        TokenValidationParameters Parameters(string key) => new()
        {
            ValidateIssuer = true,
            ValidIssuer = "MunicipalPermitSystem",
            ValidateAudience = true,
            ValidAudience = "MunicipalPermitSystem",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };

        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(token, Parameters(Key), out _); // no exception = valid

        Assert.ThrowsAny<SecurityTokenException>(() => handler.ValidateToken(token, Parameters(new string('z', 48)), out _));
    }
}
