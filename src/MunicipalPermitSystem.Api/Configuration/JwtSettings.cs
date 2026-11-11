namespace MunicipalPermitSystem.Api.Configuration;

public sealed class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "MunicipalPermitSystem";
    public string Audience { get; set; } = "MunicipalPermitSystem";
    public int ExpiryMinutes { get; set; } = 60;
}
