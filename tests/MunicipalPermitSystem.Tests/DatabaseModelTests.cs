using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MunicipalPermitSystem.Api.Configuration;
using MunicipalPermitSystem.Api.Data;
using MunicipalPermitSystem.Api.Models.Entities;
using MunicipalPermitSystem.Api.Models.Enums;
using MunicipalPermitSystem.Api.Services;

namespace MunicipalPermitSystem.Tests;

/// <summary>
/// Checks the database mapping (tables, subtypes, text enums, encryption at rest) using a throw-away
/// in-memory SQLite database — no SQL Server needed.
/// </summary>
public sealed class DatabaseModelTests : IDisposable
{
    private static readonly IFieldEncryptor Encryptor = new AesGcmFieldEncryptor(
        Options.Create(new SecuritySettings { EncryptionKey = FieldEncryptorTests.TestKey }));

    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _db;

    public DatabaseModelTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options;
        _db = new ApplicationDbContext(options, Encryptor);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    /// <summary>Reads one raw value straight from the database, bypassing EF (so we see what is really stored).</summary>
    private string Raw(string sql)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToString(command.ExecuteScalar()) ?? string.Empty;
    }

    private void AddCitizen()
    {
        _db.Citizens.Add(new Citizen
        {
            NationalId = "C1",
            FullName = "Test Citizen",
            PasswordHash = "x",
            PhoneNumber = "0791234567",
            Address = "Irbid, Jordan"
        });
        _db.SaveChanges();
    }

    [Fact]
    public void All_four_permit_kinds_load_as_the_right_type()
    {
        AddCitizen();
        _db.PermitRequests.AddRange(
            new ResidentialPermit { CitizenId = "C1", FloorType = "Tile", BalconyArea = 12.5m, FixedResidentialPrice = 1500m, NumberOfFloors = 2 },
            new CommercialPermit { CitizenId = "C1", ProjectionType = "Sign", ShopArea = 40m, FixedCommercialFee = 900m },
            new ParkingPermit { CitizenId = "C1", NumberOfParkingSpaces = 6 },
            new SpecialStructurePermit { CitizenId = "C1", StructureType = "Tower" });
        _db.SaveChanges();
        _db.ChangeTracker.Clear();

        var kinds = _db.PermitRequests.ToList().Select(p => p.GetType()).ToHashSet();

        Assert.Equal(4, kinds.Count);
        Assert.Contains(typeof(ResidentialPermit), kinds);
        Assert.Contains(typeof(CommercialPermit), kinds);
        Assert.Contains(typeof(ParkingPermit), kinds);
        Assert.Contains(typeof(SpecialStructurePermit), kinds);
    }

    [Fact]
    public void Permit_type_and_status_are_stored_as_readable_text()
    {
        AddCitizen();
        _db.PermitRequests.Add(new ParkingPermit { CitizenId = "C1", NumberOfParkingSpaces = 3 });
        _db.SaveChanges();

        Assert.Equal("Parking", Raw("SELECT PermitType FROM PermitRequests"));
        Assert.Equal("Submitted", Raw("SELECT Status FROM PermitRequests"));
    }

    [Fact]
    public void Employee_role_is_stored_as_readable_text()
    {
        _db.Employees.Add(new Employee { EmployeeId = "E1", FullName = "Sam", Role = EmployeeRole.DepartmentHead, PhoneNumber = "079" });
        _db.SaveChanges();

        Assert.Equal("DepartmentHead", Raw("SELECT Role FROM Employees"));
    }

    [Fact]
    public void Citizen_phone_and_address_are_encrypted_in_the_database()
    {
        AddCitizen();

        Assert.DoesNotContain("0791234567", Raw("SELECT PhoneNumber FROM Citizens"));
        Assert.DoesNotContain("Irbid", Raw("SELECT Address FROM Citizens"));

        _db.ChangeTracker.Clear();
        var citizen = _db.Citizens.Single();
        Assert.Equal("0791234567", citizen.PhoneNumber);
        Assert.Equal("Irbid, Jordan", citizen.Address);
    }

    [Fact]
    public void Employee_phone_is_encrypted_in_the_database()
    {
        _db.Employees.Add(new Employee { EmployeeId = "E1", FullName = "Sam", Role = EmployeeRole.Surveyor, PhoneNumber = "0790000000" });
        _db.SaveChanges();

        Assert.DoesNotContain("0790000000", Raw("SELECT PhoneNumber FROM Employees"));

        _db.ChangeTracker.Clear();
        Assert.Equal("0790000000", _db.Employees.Single().PhoneNumber);
    }

    [Fact]
    public void Money_keeps_its_exact_value()
    {
        AddCitizen();
        _db.PermitRequests.Add(new ResidentialPermit
        {
            CitizenId = "C1",
            FloorType = "Tile",
            BalconyArea = 0.10m,
            FixedResidentialPrice = 0.30m,
            NumberOfFloors = 1,
            TotalEstimatedFee = 1234567.89m
        });
        _db.SaveChanges();
        _db.ChangeTracker.Clear();

        var loaded = _db.PermitRequests.OfType<ResidentialPermit>().Single();

        Assert.Equal(1234567.89m, loaded.TotalEstimatedFee);
        Assert.Equal(0.30m, loaded.BalconyArea + 0.20m); // no floating-point drift
    }

    [Fact]
    public void Components_documents_payments_and_audit_entries_attach_to_a_request()
    {
        AddCitizen();
        var permit = new ParkingPermit { CitizenId = "C1", NumberOfParkingSpaces = 2 };
        permit.BuildingComponents.Add(new BuildingComponent { ComponentType = "Basement", AreaM2 = 80m, LengthM = 10m });
        permit.Documents.Add(new PermitDocument { DocumentType = "SitePlan", FilePath = "/tmp/plan.pdf" });
        _db.PermitRequests.Add(permit);
        _db.SaveChanges();

        _db.Payments.Add(new Payment { RequestId = permit.RequestId, Amount = 140m, PaymentMethod = PaymentMethod.Cash });
        _db.ApprovalLogs.Add(new ApprovalLog
        {
            RequestId = permit.RequestId,
            ActorType = ActorType.Employee,
            ActorId = "S1",
            EmployeeRole = EmployeeRole.Surveyor,
            Action = "ForwardedToDeptHead"
        });
        _db.SaveChanges();

        Assert.Equal("Pending", Raw("SELECT PaymentStatus FROM Payments"));
        Assert.Equal("Surveyor", Raw("SELECT EmployeeRole FROM ApprovalLogs"));
        Assert.Equal("1", Raw("SELECT COUNT(*) FROM BuildingComponents"));
        Assert.Equal("1", Raw("SELECT COUNT(*) FROM PermitDocuments"));
    }

    [Fact]
    public void A_request_with_a_payment_cannot_be_deleted()
    {
        AddCitizen();
        var permit = new ParkingPermit { CitizenId = "C1", NumberOfParkingSpaces = 2 };
        _db.PermitRequests.Add(permit);
        _db.SaveChanges();
        _db.Payments.Add(new Payment { RequestId = permit.RequestId, Amount = 10m, PaymentMethod = PaymentMethod.Cash });
        _db.SaveChanges();
        _db.ChangeTracker.Clear();

        _db.Remove(_db.PermitRequests.Single());

        Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
    }
}
