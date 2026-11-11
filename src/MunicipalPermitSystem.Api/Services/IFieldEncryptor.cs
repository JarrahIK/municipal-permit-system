namespace MunicipalPermitSystem.Api.Services;

public interface IFieldEncryptor
{
    string Encrypt(string plaintext);
    string Decrypt(string token);
}
