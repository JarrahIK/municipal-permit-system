using Microsoft.EntityFrameworkCore;
using MunicipalPermitSystem.Api.Models.Entities;
using MunicipalPermitSystem.Api.Models.Enums;
using MunicipalPermitSystem.Api.Services;

namespace MunicipalPermitSystem.Api.Data;

public class ApplicationDbContext : DbContext
{
    private readonly IFieldEncryptor _encryptor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IFieldEncryptor encryptor)
        : base(options)
    {
        _encryptor = encryptor;
    }

    public DbSet<Citizen> Citizens => Set<Citizen>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PermitRequest> PermitRequests => Set<PermitRequest>();
    public DbSet<BuildingComponent> BuildingComponents => Set<BuildingComponent>();
    public DbSet<PermitDocument> PermitDocuments => Set<PermitDocument>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ApprovalLog> ApprovalLogs => Set<ApprovalLog>();

    // Store every enum as readable text ("Surveyor", "Submitted", ...) instead of a number.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<EmployeeRole>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<PermitType>().HaveConversion<string>().HaveMaxLength(30);
        configurationBuilder.Properties<PermitStatus>().HaveConversion<string>().HaveMaxLength(30);
        configurationBuilder.Properties<PaymentMethod>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<PaymentStatus>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<ActorType>().HaveConversion<string>().HaveMaxLength(20);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var encrypted = new EncryptedStringConverter(_encryptor);

        modelBuilder.Entity<ApprovalLog>()
            .Property(a => a.EmployeeRole).HasConversion<string>().HasMaxLength(20);

        // --- people ---
        modelBuilder.Entity<Citizen>(e =>
        {
            e.Property(c => c.PhoneNumber).HasConversion(encrypted);
            e.Property(c => c.Address).HasConversion(encrypted);
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.Property(x => x.PhoneNumber).HasConversion(encrypted);
        });

        // --- permits: one table per kind, sharing the PermitRequests key ---
        modelBuilder.Entity<PermitRequest>(e =>
        {
            e.UseTptMappingStrategy();

            e.HasOne(p => p.Citizen).WithMany()
                .HasForeignKey(p => p.CitizenId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Surveyor).WithMany()
                .HasForeignKey(p => p.SurveyorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Manager).WithMany()
                .HasForeignKey(p => p.ManagerId).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(p => p.CitizenId);
            e.HasIndex(p => p.Status);

            // Components and documents belong to the request and go with it...
            e.HasMany(p => p.BuildingComponents).WithOne(b => b.PermitRequest)
                .HasForeignKey(b => b.RequestId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(p => p.Documents).WithOne(d => d.PermitRequest)
                .HasForeignKey(d => d.RequestId).OnDelete(DeleteBehavior.Cascade);

            // ...but financial and audit records must never silently disappear.
            e.HasMany(p => p.Payments).WithOne(x => x.PermitRequest)
                .HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(p => p.ApprovalLogs).WithOne(x => x.PermitRequest)
                .HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ResidentialPermit>().ToTable("ResidentialPermits");
        modelBuilder.Entity<CommercialPermit>().ToTable("CommercialPermits");
        modelBuilder.Entity<ParkingPermit>().ToTable("ParkingPermits");
        modelBuilder.Entity<SpecialStructurePermit>().ToTable("SpecialStructurePermits");
    }
}
