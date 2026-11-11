using Xunit;
using System.Security.Cryptography;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MunicipalPermitSystem.Api.Configuration;

namespace MunicipalPermitSystem.Tests;

public class StartupValidationTests
{
    private sealed class FakeEnv(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "tests";
        public string ContentRootPath { get; set; } = "/";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private static readonly JwtSettings DevJwt = new() { Key = "dev-only-insecure-jwt-key-change-me-0123456789" };
    private static readonly SecuritySettings DevSecurity = new() { EncryptionKey = FieldEncryptorTests.TestKey };
    private static JwtSettings StrongJwt() => new() { Key = new string('x', 48) };
    private static SecuritySettings StrongSecurity() => new() { EncryptionKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) };

    private static void Validate(string env, JwtSettings jwt, SecuritySettings security) =>
        StartupValidation.Validate(new FakeEnv(env), jwt, security);

    [Fact]
    public void Development_accepts_the_development_keys() => Validate("Development", DevJwt, DevSecurity);

    [Fact]
    public void Production_accepts_strong_keys() => Validate("Production", StrongJwt(), StrongSecurity());

    [Fact]
    public void Production_refuses_the_development_jwt_key() =>
        Assert.Throws<InvalidOperationException>(() => Validate("Production", DevJwt, StrongSecurity()));

    [Fact]
    public void Production_refuses_the_development_encryption_key() =>
        Assert.Throws<InvalidOperationException>(() => Validate("Production", StrongJwt(), DevSecurity));

    [Fact]
    public void A_short_or_missing_jwt_key_is_refused_everywhere()
    {
        Assert.Throws<InvalidOperationException>(() => Validate("Development", new JwtSettings { Key = "short" }, DevSecurity));
        Assert.Throws<InvalidOperationException>(() => Validate("Production", new JwtSettings(), StrongSecurity()));
    }

    [Fact]
    public void A_bad_encryption_key_is_refused()
    {
        Assert.Throws<InvalidOperationException>(() => Validate("Development", DevJwt, new SecuritySettings { EncryptionKey = "not base64!!" }));
        Assert.Throws<InvalidOperationException>(() => Validate("Development", DevJwt, new SecuritySettings { EncryptionKey = Convert.ToBase64String(new byte[16]) }));
    }
}
