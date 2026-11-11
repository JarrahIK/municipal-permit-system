namespace MunicipalPermitSystem.Api.Models.Entities;

/// <summary>What the login/lockout logic needs from a Citizen or an Employee.</summary>
public interface ILockableAccount
{
    string PasswordHash { get; }
    int FailedLoginAttempts { get; set; }
    DateTimeOffset? LockoutEnd { get; set; }
}
