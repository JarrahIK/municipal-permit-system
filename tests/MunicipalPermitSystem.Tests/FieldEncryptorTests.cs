using Xunit;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using MunicipalPermitSystem.Api.Configuration;
using MunicipalPermitSystem.Api.Services;

namespace MunicipalPermitSystem.Tests;

public class FieldEncryptorTests
{
    // Fixed test-only key: 32 bytes, base64.
    public const string TestKey = "ZGV2LW9ubHktaW5zZWN1cmUta2V5LTMyLWJ5dGVzISE=";

    private static AesGcmFieldEncryptor Make(string? key = null) =>
        new(Options.Create(new SecuritySettings { EncryptionKey = key ?? TestKey }));

    [Fact]
    public void Encrypt_then_decrypt_returns_the_original()
    {
        var enc = Make();
        Assert.Equal("0791234567", enc.Decrypt(enc.Encrypt("0791234567")));
    }

    [Fact]
    public void Same_input_gives_different_ciphertext_each_time()
    {
        var enc = Make();
        Assert.NotEqual(enc.Encrypt("0791234567"), enc.Encrypt("0791234567"));
    }

    [Fact]
    public void Ciphertext_does_not_contain_the_plaintext()
    {
        Assert.DoesNotContain("0791234567", Make().Encrypt("0791234567"));
    }

    [Fact]
    public void Empty_and_arabic_text_roundtrip()
    {
        var enc = Make();
        Assert.Equal("", enc.Decrypt(enc.Encrypt("")));
        Assert.Equal("إربد، الأردن", enc.Decrypt(enc.Encrypt("إربد، الأردن")));
    }

    [Fact]
    public void Tampered_ciphertext_is_rejected()
    {
        var enc = Make();
        var blob = Convert.FromBase64String(enc.Encrypt("secret"));
        blob[^1] ^= 1;

        Assert.ThrowsAny<CryptographicException>(() => enc.Decrypt(Convert.ToBase64String(blob)));
    }

    [Fact]
    public void A_different_key_cannot_decrypt()
    {
        var token = Make().Encrypt("secret");
        var otherKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        Assert.ThrowsAny<CryptographicException>(() => Make(otherKey).Decrypt(token));
    }

    [Fact]
    public void A_key_that_is_not_32_bytes_is_refused()
    {
        Assert.Throws<InvalidOperationException>(() => Make(Convert.ToBase64String(new byte[16])));
    }
}
