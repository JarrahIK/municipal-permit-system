namespace MunicipalPermitSystem.Api.Services;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateToken(string subjectId, string name, string role);
}
