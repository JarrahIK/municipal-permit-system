using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MunicipalPermitSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Citizens",
                columns: table => new
                {
                    NationalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citizens", x => x.NationalId);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                });

            migrationBuilder.CreateTable(
                name: "PermitRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CitizenId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SurveyorId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ManagerId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PermitType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SubmissionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TotalEstimatedFee = table.Column<decimal>(type: "decimal(19,2)", nullable: false),
                    IsViolationPresent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermitRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_PermitRequests_Citizens_CitizenId",
                        column: x => x.CitizenId,
                        principalTable: "Citizens",
                        principalColumn: "NationalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PermitRequests_Employees_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PermitRequests_Employees_SurveyorId",
                        column: x => x.SurveyorId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalLogs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    ActorType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmployeeRole = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_ApprovalLogs_PermitRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "PermitRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BuildingComponents",
                columns: table => new
                {
                    ComponentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    ComponentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AreaM2 = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    LengthM = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsViolation = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingComponents", x => x.ComponentId);
                    table.ForeignKey(
                        name: "FK_BuildingComponents_PermitRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "PermitRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommercialPermits",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    ProjectionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShopArea = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FixedCommercialFee = table.Column<decimal>(type: "decimal(19,2)", nullable: false),
                    NumberOfFloors = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommercialPermits", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_CommercialPermits_PermitRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "PermitRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParkingPermits",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    NumberOfParkingSpaces = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingPermits", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_ParkingPermits_PermitRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "PermitRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransactionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BlockchainHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_PermitRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "PermitRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PermitDocuments",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UploadDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermitDocuments", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_PermitDocuments_PermitRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "PermitRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResidentialPermits",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    FloorType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BalconyArea = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FixedResidentialPrice = table.Column<decimal>(type: "decimal(19,2)", nullable: false),
                    NumberOfFloors = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidentialPermits", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_ResidentialPermits_PermitRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "PermitRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecialStructurePermits",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    StructureType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialStructurePermits", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_SpecialStructurePermits_PermitRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "PermitRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalLogs_RequestId",
                table: "ApprovalLogs",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingComponents_RequestId",
                table: "BuildingComponents",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_RequestId",
                table: "Payments",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PermitDocuments_RequestId",
                table: "PermitDocuments",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PermitRequests_CitizenId",
                table: "PermitRequests",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_PermitRequests_ManagerId",
                table: "PermitRequests",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_PermitRequests_Status",
                table: "PermitRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PermitRequests_SurveyorId",
                table: "PermitRequests",
                column: "SurveyorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalLogs");

            migrationBuilder.DropTable(
                name: "BuildingComponents");

            migrationBuilder.DropTable(
                name: "CommercialPermits");

            migrationBuilder.DropTable(
                name: "ParkingPermits");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PermitDocuments");

            migrationBuilder.DropTable(
                name: "ResidentialPermits");

            migrationBuilder.DropTable(
                name: "SpecialStructurePermits");

            migrationBuilder.DropTable(
                name: "PermitRequests");

            migrationBuilder.DropTable(
                name: "Citizens");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
