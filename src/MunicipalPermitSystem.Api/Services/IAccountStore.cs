using MunicipalPermitSystem.Api.Models.Entities;

namespace MunicipalPermitSystem.Api.Services;

/// <summary>
/// Where citizen / employee accounts are kept. The login logic talks to this instead of the
/// database directly, so it can be unit-tested without a database.
/// </summary>
public interface IAccountStore
{
    Task<Citizen?> FindCitizenAsync(string nationalId, CancellationToken ct = default);
    Task<Employee?> FindEmployeeAsync(string employeeId, CancellationToken ct = default);
    Task<bool> CitizenExistsAsync(string nationalId, CancellationToken ct = default);

    /// <summary>Adds the citizen. Returns false (and adds nothing) if that National ID is already registered.</summary>
    Task<bool> TryAddCitizenAsync(Citizen citizen, CancellationToken ct = default);

    /// <summary>Saves changes made to accounts previously returned by Find*, e.g. the failed-login counter.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
