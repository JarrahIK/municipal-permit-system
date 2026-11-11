using Microsoft.Extensions.Options;
using MunicipalPermitSystem.Api.Configuration;

namespace MunicipalPermitSystem.Api.Services;

/// <summary>bcrypt password hashing (NFR1.2: passwords are never stored in plain text).</summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    // A real bcrypt hash of a throwaway value, used to equalize timing for unknown IDs.
    private static readonly string DummyHash =
        BCrypt.Net.BCrypt.HashPassword("not-a-real-password-just-for-timing", 11);

    private readonly int _workFactor;

    public BcryptPasswordHasher(IOptions<SecuritySettings> security)
    {
        _workFactor = security.Value.BcryptWorkFactor;
    }

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, _workFactor);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);

    public void SpendTimeLikeAVerify(string password) => BCrypt.Net.BCrypt.Verify(password, DummyHash);
}
