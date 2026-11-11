using Microsoft.EntityFrameworkCore;
using MunicipalPermitSystem.Api.Models.Entities;
using MunicipalPermitSystem.Api.Models.Enums;

namespace MunicipalPermitSystem.Api.Data;

/// <summary>
/// Creates one demo account per role so you can log in right after the first run.
/// Only runs when Seed:DemoData is true (Development) — Program.cs refuses it in Production.
/// </summary>
public static class DevDataSeeder
{
    public const string DemoPassword = "DevPass123!";

    private static readonly (string Id, string Name, EmployeeRole Role)[] DemoStaff =
    {
        ("SURV001", "Demo Surveyor", EmployeeRole.Surveyor),
        ("HEAD001", "Demo Department Head", EmployeeRole.DepartmentHead),
        ("MGR001", "Demo Manager", EmployeeRole.Manager),
        ("ACC001", "Demo Accountant", EmployeeRole.Accountant),
    };

    public const string DemoCitizenNationalId = "1000000001";

    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(DemoPassword);

        foreach (var (id, name, role) in DemoStaff)
        {
            if (await db.Employees.AnyAsync(e => e.EmployeeId == id, ct)) continue;

            db.Employees.Add(new Employee
            {
                EmployeeId = id,
                FullName = name,
                Role = role,
                Department = "Building Permits",
                PhoneNumber = "0790000000",
                PasswordHash = hash
            });
        }

        if (!await db.Citizens.AnyAsync(c => c.NationalId == DemoCitizenNationalId, ct))
        {
            db.Citizens.Add(new Citizen
            {
                NationalId = DemoCitizenNationalId,
                FullName = "Demo Citizen",
                PhoneNumber = "0791111111",
                Address = "Irbid, Jordan",
                PasswordHash = hash
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
