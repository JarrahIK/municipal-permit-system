using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MunicipalPermitSystem.Api.Services;

namespace MunicipalPermitSystem.Api.Data;

/// <summary>
/// Makes a string column transparently AES-256-GCM encrypted in the database (NFR1.2): your code
/// reads and writes normal strings, the database only ever sees ciphertext.
/// Encrypted columns can't be searched or indexed by value — use only for data we display.
/// </summary>
public sealed class EncryptedStringConverter : ValueConverter<string, string>
{
    public EncryptedStringConverter(IFieldEncryptor encryptor)
        : base(v => encryptor.Encrypt(v), v => encryptor.Decrypt(v))
    {
    }
}
