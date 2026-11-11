namespace MunicipalPermitSystem.Api.Services;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);

    /// <summary>Spends the same time as a real <see cref="Verify"/> — used when the account doesn't exist.</summary>
    void SpendTimeLikeAVerify(string password);
}
