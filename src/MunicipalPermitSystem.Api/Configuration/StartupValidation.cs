using Microsoft.Extensions.Hosting;

namespace MunicipalPermitSystem.Api.Configuration;

/// <summary>
/// Fails fast, with a clear message, when secrets are missing or insecure — and refuses to run
/// outside Development with the built-in dev-only keys.
/// </summary>
public static class StartupValidation
{
    private const string DevJwtPrefix = "dev-only";
    private const string DevEncryptionKey = "ZGV2LW9ubHktaW5zZWN1cmUta2V5LTMyLWJ5dGVzISE=";

    public static void Validate(IHostEnvironment env, JwtSettings jwt, SecuritySettings security)
    {
        if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
            throw new InvalidOperationException(
                "Jwt:Key must be set and at least 32 characters long " +
                "(environment variable Jwt__Key in non-Development environments).");

        byte[] key;
        try
        {
            key = Convert.FromBase64String(security.EncryptionKey ?? string.Empty);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("Security:EncryptionKey must be valid base64.");
        }

        if (key.Length != 32)
            throw new InvalidOperationException(
                "Security:EncryptionKey must be base64 of exactly 32 bytes (AES-256).");

        if (!env.IsDevelopment())
        {
            if (jwt.Key.StartsWith(DevJwtPrefix, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Refusing to start: Jwt:Key is still the insecure development key.");

            if (security.EncryptionKey == DevEncryptionKey)
                throw new InvalidOperationException(
                    "Refusing to start: Security:EncryptionKey is still the insecure development key.");
        }
    }
}
