using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MunicipalPermitSystem.Api.Configuration;

namespace MunicipalPermitSystem.Api.Services;

/// <summary>
/// AES-256-GCM field encryption (NFR1.2). Output = Base64( nonce[12] + ciphertext + tag[16] ).
/// A fresh random nonce is used every time, and any tampering is detected on decrypt.
/// </summary>
public sealed class AesGcmFieldEncryptor : IFieldEncryptor
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _key;

    public AesGcmFieldEncryptor(IOptions<SecuritySettings> options)
    {
        _key = Convert.FromBase64String(options.Value.EncryptionKey);
        if (_key.Length != 32)
            throw new InvalidOperationException("Security:EncryptionKey must decode to exactly 32 bytes (AES-256).");
    }

    public string Encrypt(string plaintext)
    {
        var plain = Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[plain.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plain, cipher, tag);

        var blob = new byte[NonceSize + cipher.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, blob, 0, NonceSize);
        Buffer.BlockCopy(cipher, 0, blob, NonceSize, cipher.Length);
        Buffer.BlockCopy(tag, 0, blob, NonceSize + cipher.Length, TagSize);
        return Convert.ToBase64String(blob);
    }

    public string Decrypt(string token)
    {
        var blob = Convert.FromBase64String(token);
        if (blob.Length < NonceSize + TagSize)
            throw new CryptographicException("Encrypted value is too short.");

        var nonce = blob.AsSpan(0, NonceSize);
        var cipher = blob.AsSpan(NonceSize, blob.Length - NonceSize - TagSize);
        var tag = blob.AsSpan(blob.Length - TagSize, TagSize);
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipher, tag, plain);
        return Encoding.UTF8.GetString(plain);
    }
}
