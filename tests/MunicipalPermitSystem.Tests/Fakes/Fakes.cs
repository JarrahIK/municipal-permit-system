using MunicipalPermitSystem.Api.Models.Entities;
using MunicipalPermitSystem.Api.Services;

namespace MunicipalPermitSystem.Tests.Fakes;

/// <summary>A clock the test controls, so "15 minutes later" doesn't mean waiting 15 minutes.</summary>
public sealed class FakeClock : TimeProvider
{
    private DateTimeOffset _now;

    public FakeClock(DateTimeOffset? start = null)
    {
        _now = start ?? new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    }

    public DateTimeOffset Now => _now;
    public void Advance(TimeSpan by) => _now += by;
    public override DateTimeOffset GetUtcNow() => _now;
}

/// <summary>Accounts kept in memory instead of a database.</summary>
public sealed class FakeAccountStore : IAccountStore
{
    public Dictionary<string, Citizen> Citizens { get; } = new();
    public Dictionary<string, Employee> Employees { get; } = new();
    public int SaveCount { get; private set; }

    public Task<Citizen?> FindCitizenAsync(string nationalId, CancellationToken ct = default)
        => Task.FromResult(Citizens.GetValueOrDefault(nationalId));

    public Task<Employee?> FindEmployeeAsync(string employeeId, CancellationToken ct = default)
        => Task.FromResult(Employees.GetValueOrDefault(employeeId));

    public Task<bool> CitizenExistsAsync(string nationalId, CancellationToken ct = default)
        => Task.FromResult(Citizens.ContainsKey(nationalId));

    public Task<bool> TryAddCitizenAsync(Citizen citizen, CancellationToken ct = default)
        => Task.FromResult(Citizens.TryAdd(citizen.NationalId, citizen));

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}

/// <summary>Instant, predictable "hashing" so tests don't spend time on bcrypt.</summary>
public sealed class FakePasswordHasher : IPasswordHasher
{
    public int VerifyCalls { get; private set; }
    public int DummyVerifyCalls { get; private set; }

    public string Hash(string password) => "hash::" + password;

    public bool Verify(string password, string hash)
    {
        VerifyCalls++;
        return hash == "hash::" + password;
    }

    public void SpendTimeLikeAVerify(string password) => DummyVerifyCalls++;
}

public sealed class FakeJwtTokenService : IJwtTokenService
{
    public (string Token, DateTimeOffset ExpiresAt) CreateToken(string subjectId, string name, string role)
        => ($"token:{subjectId}:{role}", DateTimeOffset.UnixEpoch);
}
