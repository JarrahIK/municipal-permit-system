namespace MunicipalPermitSystem.Api.Configuration;

public sealed class SecuritySettings
{
    /// <summary>Base64 of exactly 32 random bytes — the AES-256-GCM key for encrypted columns (NFR1.2).</summary>
    public string EncryptionKey { get; set; } = string.Empty;

    /// <summary>bcrypt cost. 11 is a good default; tests lower it so they run fast.</summary>
    public int BcryptWorkFactor { get; set; } = 11;

    /// <summary>NFR1.1: lock the account after this many failed logins...</summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>...for this many minutes.</summary>
    public int LockoutMinutes { get; set; } = 15;
}
