using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClinicManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Services__C51BB0EA0B0C46B1", x => x.ServiceID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    VendorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Vendors__FC8653AD1D3DE96A", x => x.VendorID);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BloodType = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Patients__970EC346E8240D4B", x => x.PatientID);
                    table.ForeignKey(
                        name: "FK_Patients_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffDetails",
                columns: table => new
                {
                    StaffID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StaffDet__96D4AAF741B20436", x => x.StaffID);
                    table.ForeignKey(
                        name: "FK_StaffDetails_Users_UserId_New",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SellingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VendorID = table.Column<int>(type: "int", nullable: false),
                    ReorderLevel = table.Column<int>(type: "int", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Inventory__7BF7D5B66736C561", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK__Inventory__Vendo__6EF57B66",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "VendorID");
                });

            migrationBuilder.CreateTable(
                name: "VerificationCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ContactMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationCodes_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    AppointmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientID = table.Column<int>(type: "int", nullable: true),
                    DoctorID = table.Column<int>(type: "int", nullable: true),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    AppointmentDateTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, defaultValueSql: "('Scheduled')"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Appointm__8ECDFCCC56AF500B", x => x.AppointmentID);
                    table.ForeignKey(
                        name: "FK__Appointme__Docto__5CD626F7",
                        column: x => x.DoctorID,
                        principalTable: "StaffDetails",
                        principalColumn: "StaffID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK__Appointme__Patie__5BE2A6F2",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK__Appointme__Servi__5DCAEF64",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID");
                });

            migrationBuilder.CreateTable(
                name: "ItemBatches",
                columns: table => new
                {
                    BatchID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemID = table.Column<int>(type: "int", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "date", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "date", nullable: false),
                    CostPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ItemBatc__B28D3F372095039A", x => x.BatchID);
                    table.ForeignKey(
                        name: "FK_ItemBatches_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "VendorID");
                    table.ForeignKey(
                        name: "FK__ItemBatch__ItemI__72C60C4A",
                        column: x => x.ItemID,
                        principalTable: "InventoryItems",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecords",
                columns: table => new
                {
                    RecordID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientID = table.Column<int>(type: "int", nullable: false),
                    AppointmentID = table.Column<int>(type: "int", nullable: true),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Treatment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Prescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MedicalRecordRecordId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MedicalR__FBDF78C62A86B91A", x => x.RecordID);
                    table.ForeignKey(
                        name: "FK_MedicalRecords_MedicalRecords_MedicalRecordRecordId",
                        column: x => x.MedicalRecordRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "RecordID");
                    table.ForeignKey(
                        name: "FK_MedicalRecords_StaffDetails_StaffID",
                        column: x => x.StaffID,
                        principalTable: "StaffDetails",
                        principalColumn: "StaffID");
                    table.ForeignKey(
                        name: "FK__MedicalRe__Appoi__60A75C0F",
                        column: x => x.AppointmentID,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentID");
                    table.ForeignKey(
                        name: "FK__MedicalRe__Patie__5FB337D6",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientID");
                    table.ForeignKey(
                        name: "FK__MedicalRe__Servi__XXXXXX",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StockTransactions",
                columns: table => new
                {
                    TransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    StaffId = table.Column<int>(type: "int", nullable: true),
                    PatientId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StockTra__55433B6A7F2D065C", x => x.TransactionID);
                    table.ForeignKey(
                        name: "FK_StockTransactions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientID");
                    table.ForeignKey(
                        name: "FK_StockTransactions_StaffDetails_StaffId",
                        column: x => x.StaffId,
                        principalTable: "StaffDetails",
                        principalColumn: "StaffID");
                    table.ForeignKey(
                        name: "FK__StockTran__Batch__75A278FBC",
                        column: x => x.BatchID,
                        principalTable: "ItemBatches",
                        principalColumn: "BatchID");
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2023, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Admin", "ADMIN", new DateTime(2023, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, null, new DateTime(2023, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Receptionist", "RECEPTIONIST", new DateTime(2023, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, null, new DateTime(2023, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "HR", "HR", new DateTime(2023, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, null, new DateTime(2023, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Doctor", "DOCTOR", new DateTime(2023, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, null, new DateTime(2023, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Nurse", "NURSE", new DateTime(2023, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, null, new DateTime(2023, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Patient", "PATIENT", new DateTime(2023, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, null, new DateTime(2023, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "InventoryManager", "INVENTORYMANAGER", new DateTime(2023, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "ServiceID", "CreatedAt", "Description", "Price", "ServiceName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Routine physicals, well-woman exams, and age-appropriate screenings for conditions like diabetes or high blood pressure.", 100m, "General Checkups and Screenings", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Addressing colds, flu, ear infections, skin rashes, and other minor ailments.", 100m, "Treatment of Common Illnesses", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Providing vaccinations for children and adults, including flu shots and other recommended immunizations.", 100m, "Vaccinations", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Helping patients manage conditions like diabetes, hypertension, and asthma. ", 100m, "Chronic Disease Management", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Care for heart conditions.", 300m, "Cardiology", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Skin-related issues and treatments.", 300m, "Dermatology", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Musculoskeletal problems and injuries.", 300m, "Orthopedics", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Rehabilitation and pain management.", 300m, "Physical Therapy", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Foot and ankle care.", 300m, "Podiatry", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Treatment of related conditions.", 300m, "ENT (Ear, Nose, and Throat)", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Women's health and prenatal care.", 300m, "Gynecology and Obstetrics", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Eye care.", 300m, "Ophthalmology", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Oral health and dental procedures.", 300m, "Dentistry", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 14, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Counseling, therapy, and psychiatric care.", 200m, "Mental Health Services", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 15, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Support for substance abuse and addiction recovery.", 200m, "Addiction Services", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Blood work, urine tests, imaging (X-rays, ultrasounds), and other diagnostic procedures.", 200m, "Laboratory and Diagnostic Services", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "StaffDetails",
                columns: new[] { "StaffID", "ContactNumber", "CreatedAt", "Email", "FirstName", "JobTitle", "LastName", "MiddleName", "Specialization", "UpdatedAt", "UserID" },
                values: new object[,]
                {
                    { 101, "09123456789", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "ava.chen@clinic.com", "Ava", "General Practitioner", "Chen", null, "Family Medicine", null, null },
                    { 102, "09234567890", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "ben.roberts@clinic.com", "Ben", "General Practitioner", "Roberts", null, "Internal Medicine", null, null },
                    { 103, "09345678901", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "clara.garcia@clinic.com", "Clara", "Cardiologist", "Garcia", null, "Cardiology", null, null },
                    { 104, "09456789012", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "david.lee@clinic.com", "David", "Dermatologist", "Lee", null, "Dermatology", null, null },
                    { 105, "09567890123", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "emily.wang@clinic.com", "Emily", "Orthopedic Surgeon", "Wang", null, "Orthopedics", null, null },
                    { 106, "09678901234", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "frank.miller@clinic.com", "Frank", "Physical Therapist", "Miller", null, "Physical Therapy", null, null },
                    { 107, "09789012345", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "grace.kim@clinic.com", "Grace", "Podiatrist", "Kim", null, "Podiatry", null, null },
                    { 108, "09890123456", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "henry.nguyen@clinic.com", "Henry", "ENT Specialist", "Nguyen", null, "Otorhinolaryngology (ENT)", null, null },
                    { 109, "09901234567", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "isabella.patel@clinic.com", "Isabella", "Obstetrician-Gynecologist", "Patel", null, "Gynecology & Obstetrics", null, null },
                    { 110, "09012345678", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "jack.davis@clinic.com", "Jack", "Ophthalmologist", "Davis", null, "Ophthalmology", null, null },
                    { 111, "09102345678", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "kara.lopez@clinic.com", "Kara", "Dentist", "Lopez", null, "Dentistry", null, null },
                    { 112, "09213456789", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "liam.martinez@clinic.com", "Liam", "Psychiatrist", "Martinez", null, "Psychiatry", null, null },
                    { 113, "09324567890", new DateTime(2025, 6, 23, 10, 0, 0, 0, DateTimeKind.Utc), "mia.wilson@clinic.com", "Mia", "Addiction Counselor", "Wilson", null, "Addiction Medicine", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorID",
                table: "Appointments",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientID",
                table: "Appointments",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceID",
                table: "Appointments",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_VendorID",
                table: "InventoryItems",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBatches_ItemID",
                table: "ItemBatches",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBatches_VendorId",
                table: "ItemBatches",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_AppointmentID",
                table: "MedicalRecords",
                column: "AppointmentID");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_MedicalRecordRecordId",
                table: "MedicalRecords",
                column: "MedicalRecordRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientID",
                table: "MedicalRecords",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_ServiceID",
                table: "MedicalRecords",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_StaffID",
                table: "MedicalRecords",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_ContactNumber",
                table: "Patients",
                column: "ContactNumber",
                unique: true,
                filter: "[ContactNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Email",
                table: "Patients",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_UserId",
                table: "Patients",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StaffDetails_UserID",
                table: "StaffDetails",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_BatchID",
                table: "StockTransactions",
                column: "BatchID");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_PatientId",
                table: "StockTransactions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_StaffId",
                table: "StockTransactions",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationCodes_PatientId",
                table: "VerificationCodes",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalRecords");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "StockTransactions");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "VerificationCodes");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "ItemBatches");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "StaffDetails");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Vendors");
        }
    }
}
