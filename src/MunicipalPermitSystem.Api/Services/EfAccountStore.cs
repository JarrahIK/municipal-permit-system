using Microsoft.EntityFrameworkCore;
using MunicipalPermitSystem.Api.Data;
using MunicipalPermitSystem.Api.Models.Entities;

namespace MunicipalPermitSystem.Api.Services;

/// <summary>The real account store, backed by the database (EF Core).</summary>
public sealed class EfAccountStore : IAccountStore
{
    private readonly ApplicationDbContext _db;

    public EfAccountStore(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Citizen?> FindCitizenAsync(string nationalId, CancellationToken ct = default)
        => _db.Citizens.FirstOrDefaultAsync(c => c.NationalId == nationalId, ct);

    public Task<Employee?> FindEmployeeAsync(string employeeId, CancellationToken ct = default)
        => _db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId, ct);

    public Task<bool> CitizenExistsAsync(string nationalId, CancellationToken ct = default)
        => _db.Citizens.AnyAsync(c => c.NationalId == nationalId, ct);

    public async Task<bool> TryAddCitizenAsync(Citizen citizen, CancellationToken ct = default)
    {
        _db.Citizens.Add(citizen);
        try
        {
            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException)
        {
            // Two people registering the same ID at the same moment: the second one lands here.
            _db.Entry(citizen).State = EntityState.Detached;
            if (await _db.Citizens.AnyAsync(c => c.NationalId == citizen.NationalId, ct))
                return false;
            throw;
        }
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
