using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ClinicManagement.Data.Models;
using System.Security.Cryptography.X509Certificates; // Ensure all your custom models are in this namespace

namespace ClinicManagement.Data.Context
{
    public partial class ClinicManagementDbContext : IdentityDbContext<
        User,
        Role,
        int,
        IdentityUserClaim<int>,
        UserRole, // Your custom UserRole
        IdentityUserLogin<int>,
        IdentityRoleClaim<int>,
        IdentityUserToken<int>>
    {
        public ClinicManagementDbContext(DbContextOptions<ClinicManagementDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Appointment> Appointments { get; set; } = null!;
        public virtual DbSet<MedicalRecord> MedicalRecords { get; set; } = null!;
        public virtual DbSet<Patient> Patients { get; set; } = null!;
        public virtual DbSet<VerificationCode> VerificationCodes { get; set; } = null!;
        public new virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Service> Services { get; set; } = null!;
        public virtual DbSet<StaffDetail> StaffDetails { get; set; } = null!;
        public new virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Vendor> Vendors { get; set; } = null!;
        public virtual DbSet<InventoryItem> InventoryItems { get; set; } = null!;
        public virtual DbSet<ItemBatch> ItemBatches { get; set; } = null!;
        public virtual DbSet<StockTransaction> StockTransactions { get; set; } = null!;
        public virtual DbSet<ClinicSetting> ClinicSettings { get; set; } = null!;
        public virtual DbSet<TriageRecord> TriageRecords { get; set; } = null!; 
        public virtual DbSet<LabResult> LabResults { get; set; } = null!;
        public virtual DbSet<PatientDocument> PatientDocuments { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // CRITICAL: This must be called first

            // --- Configure Identity Tables to use your custom names ---
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");

            // --- Explicitly configure the IdentityUserRole<int> (UserRoles) entity ---
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                entity.HasOne(ur => ur.User)
                      .WithMany(u => u.UserRoles)
                      .HasForeignKey(ur => ur.UserId)
                      .IsRequired();
                entity.HasOne(ur => ur.Role)
                      .WithMany(r => r.UserRoles)
                      .HasForeignKey(ur => ur.RoleId)
                      .IsRequired();
                entity.ToTable("UserRoles");
            });

            // --- Configure User's custom properties ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
                // IsActive is a simple bool, no special fluent API needed unless you have defaults/conversions.
            });

            // --- Configure Role's custom properties and seeding ---
            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
                entity.HasData(
                    new Role { Id = 1, Name = "Admin", NormalizedName = "ADMIN", CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
                    new Role { Id = 2, Name = "Receptionist", NormalizedName = "RECEPTIONIST", CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
                    new Role { Id = 3, Name = "HR", NormalizedName = "HR", CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
                    new Role { Id = 4, Name = "Doctor", NormalizedName = "DOCTOR", CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
                    new Role { Id = 5, Name = "Nurse", NormalizedName = "NURSE", CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
                    new Role { Id = 6, Name = "Patient", NormalizedName = "PATIENT", CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
                    new Role { Id = 7, Name = "InventoryManager", NormalizedName = "INVENTORYMANAGER", CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) }
                );
            });

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.PatientId).HasName("PK__Patients__970EC346E8240D4B");
                entity.Property(e => e.PatientId).HasColumnName("PatientID");

                // Keep explicit column name mapping for UserId, but it's just a regular column now
                entity.Property(e => e.UserId).HasColumnName("UserId");

                // Your existing query filter
                entity.HasQueryFilter(p => !p.IsDeleted);

                // Add unique indexes back (confirm these are present)
                entity.HasIndex(e => e.ContactNumber).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                // --- CRITICAL FIX: Add this line to explicitly make UserId index non-unique ---
                entity.HasIndex(e => e.UserId); // This explicitly creates a non-unique index on UserId

                // ... (all other property mappings for Patient like FirstName, LastName, etc. should be here) ...
                // ... (any other relationships from Patient to Appointment, MedicalRecord etc. that DON'T involve User directly) ...
            });


            // --- StaffDetail entity configuration (confirming existing setup) ---
            modelBuilder.Entity<StaffDetail>(entity =>
            {
                entity.HasKey(e => e.StaffId).HasName("PK__StaffDet__96D4AAF741B20436");
                entity.Property(e => e.StaffId).HasColumnName("StaffID");
                entity.Property(e => e.ContactNumber).HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.JobTitle).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.MiddleName).HasMaxLength(50);
                entity.Property(e => e.Specialization).HasMaxLength(100); // Ensure this property exists in your StaffDetail model
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
                entity.Property(e => e.UserId).HasColumnName("UserID"); // Ensure this maps to DB column
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                // Relationship StaffDetail to User (staff login)
                entity.HasOne(sd => sd.User)
                    .WithMany(u => u.StaffDetails) // This assumes 'User' has `ICollection<StaffDetail> StaffDetails`
                    .HasForeignKey(sd => sd.UserId)
                    .HasPrincipalKey(u => u.Id)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_StaffDetails_Users_UserId_New");

                // --- Add StaffDetail (Doctor) Seeding ---
                entity.HasData(
                    new StaffDetail { StaffId = 101, FirstName = "Ava", LastName = "Chen", JobTitle = "General Practitioner", Specialization = "Family Medicine", ContactNumber = "09123456789", Email = "ava.chen@clinic.com", CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                    new StaffDetail { StaffId = 102, FirstName = "Ben", LastName = "Roberts", JobTitle = "General Practitioner", Specialization = "Internal Medicine", ContactNumber = "09234567890", Email = "ben.roberts@clinic.com", CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                    new StaffDetail { StaffId = 103, FirstName = "Clara", LastName = "Garcia", JobTitle = "Cardiologist", Specialization = "Cardiology", ContactNumber = "09345678901", Email = "clara.garcia@clinic.com", CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                    new StaffDetail { StaffId = 104, FirstName = "David", LastName = "Lee", JobTitle = "Dermatologist", Specialization = "Dermatology", ContactNumber = "09456789012", Email = "david.lee@clinic.com", CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                    new StaffDetail { StaffId = 105, FirstName = "Emily", LastName = "Wang", JobTitle = "Orthopedic Surgeon", Specialization = "Orthopedics", ContactNumber = "09567890123", Email = "emily.wang@clinic.com", CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                    new StaffDetail { StaffId = 106, FirstName = "Frank", LastName = "Miller", JobTitle = "Physical Therapist", Specialization = "Physical Therapy", ContactNumber = "09678901234", Email = "frank.miller@clinic.com", CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                    new StaffDetail { StaffId = 107, FirstName = "Grace", LastName = "Kim", JobTitle = "Podiatrist", Specialization = "Podiatry", ContactNumber = "09789012345", Email = "grace.kim@clinic.com", CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                    new StaffDetail { StaffId = 108, FirstName = "Henry", LastName = "Nguyen", JobTitle = "ENT Specialist", Specialization = "Otorhinolaryngology (ENT)", ContactNumber = "09890123456", Email = "henry.nguyen@clinic.com", CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                    new StaffDetail { StaffId = 109, FirstName = "Isabella", LastName = "Patel", JobTitle = "Obstetrician-Gynecologist", Specialization = "Gynecology & Obstetrics", ContactNumber = "09901234567", Email = "isabella.patel@clinic.com", CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                    new StaffDetail { StaffId = 110, FirstName = "Jack", LastName = "Davis", JobTitle = "Ophthalmologist", Specialization = "Ophthalmology", ContactNumber = "09012345678", Email = "jack.davis@clinic.com", CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                    new StaffDetail { StaffId = 111, FirstName = "Kara", LastName = "Lopez", JobTitle = "Dentist", Specialization = "Dentistry", ContactNumber = "09102345678", Email = "kara.lopez@clinic.com", CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                    new StaffDetail { StaffId = 112, FirstName = "Liam", LastName = "Martinez", JobTitle = "Psychiatrist", Specialization = "Psychiatry", ContactNumber = "09213456789", Email = "liam.martinez@clinic.com", CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                    new StaffDetail { StaffId = 113, FirstName = "Mia", LastName = "Wilson", JobTitle = "Addiction Counselor", Specialization = "Addiction Medicine", ContactNumber = "09324567890", Email = "mia.wilson@clinic.com", CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), IsDeleted = false },
                    new StaffDetail { StaffId = 114, FirstName = "Allysa", LastName = "Dear", JobTitle = "Receptionist", ContactNumber = "09324567891", Email = "allysa.dear@clinic.com", CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), IsDeleted = false }
                );
            });

            // --- Configure TriageRecord entity ---
            modelBuilder.Entity<TriageRecord>(entity =>
            {
                entity.HasKey(e => e.TriageRecordId); // Use TriageRecordId as primary key
                entity.Property(e => e.TriageRecordId).HasColumnName("TriageRecordID");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                // Relationships for TriageRecord
                entity.HasOne(tr => tr.Patient)
                    .WithMany() // Assuming a patient can have many triage records, but a record belongs to one patient
                    .HasForeignKey(tr => tr.PatientId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict) // Prevent cascading delete of patient
                    .HasConstraintName("FK_TriageRecords_Patients"); // Custom constraint name

                entity.HasOne(tr => tr.Appointment)
                    .WithMany() // Assuming an appointment can have many triage records (less common) or one-to-one (more common)
                    .HasForeignKey(tr => tr.AppointmentId)
                    .IsRequired(false) // Appointment link is optional
                    .OnDelete(DeleteBehavior.SetNull) // Set FK to null if appointment is deleted
                    .HasConstraintName("FK_TriageRecords_Appointments"); // Custom constraint name

                // Configure decimal precision for vital signs
                entity.Property(e => e.Temperature).HasColumnType("decimal(4, 1)"); // e.g., 37.0
                entity.Property(e => e.Weight).HasColumnType("decimal(6, 2)");    // e.g., 70.50
                entity.Property(e => e.Height).HasColumnType("decimal(5, 2)");    // e.g., 175.25

                // Apply global query filter for soft delete
                entity.HasQueryFilter(tr => !tr.IsDeleted);
            });

            // --- Appointment entity configuration (confirming existing setup) ---
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCCC56AF500B");
                entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
                entity.Property(e => e.AppointmentDateTime).HasColumnType("datetime");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.PatientId).HasColumnName("PatientID");
                entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Scheduled')");

                entity.HasOne(d => d.Service).WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Appointme__Servi__5DCAEF64");

                entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.DoctorId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__Appointme__Docto__5CD626F7");

                entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.PatientId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__Appointme__Patie__5BE2A6F2");
            });

            // --- Medical Record entity configuration (confirming existing setup) ---
            modelBuilder.Entity<MedicalRecord>(entity =>
            {
                entity.HasKey(e => e.RecordId).HasName("PK__MedicalR__FBDF78C62A86B91A");
                entity.Property(e => e.RecordId).HasColumnName("RecordID");
                entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.Diagnosis).HasMaxLength(500);
                entity.Property(e => e.PatientId).HasColumnName("PatientID");
                entity.Property(e => e.Prescription).HasMaxLength(1000);
                entity.Property(e => e.Treatment).HasMaxLength(1000);
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
                entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
                entity.Property(e => e.StaffId).HasColumnName("StaffID");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasQueryFilter(mr => mr.Patient == null || !mr.Patient.IsDeleted);

                entity.HasOne(d => d.Appointment).WithMany(p => p.MedicalRecords)
                    .HasForeignKey(d => d.AppointmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__MedicalRe__Appoi__60A75C0F");

                entity.HasOne(d => d.Patient).WithMany(p => p.MedicalRecords)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__MedicalRe__Patie__5FB337D6");

                entity.HasOne(d => d.Staff).WithMany(p => p.MedicalRecords)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MedicalRecords_StaffDetails_StaffID");

                entity.HasOne(d => d.Service).WithMany(p => p.MedicalRecords)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__MedicalRe__Servi__XXXXXX"); // Placeholder constraint name
            });

            // --- Service entity configuration (confirming existing setup) ---
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.ServiceId).HasName("PK__Services__C51BB0EA0B0C46B1");
                entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.ServiceName).HasMaxLength(100);
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                // --- Add Service Seeding ---
                entity.HasData(
                    new Service { ServiceId = 1, ServiceName = "General Checkups and Screenings", Description = "Routine physicals, well-woman exams, and age-appropriate screenings for conditions like diabetes or high blood pressure.", Price = 100, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 2, ServiceName = "Treatment of Common Illnesses", Description = "Addressing colds, flu, ear infections, skin rashes, and other minor ailments.", Price = 100, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 3, ServiceName = "Vaccinations", Description = "Providing vaccinations for children and adults, including flu shots and other recommended immunizations.", Price = 100, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 4, ServiceName = "Chronic Disease Management", Description = "Helping patients manage conditions like diabetes, hypertension, and asthma. ", Price = 100, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 5, ServiceName = "Cardiology", Description = "Care for heart conditions.", Price = 300, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 6, ServiceName = "Dermatology", Description = "Skin-related issues and treatments.", Price = 300, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 7, ServiceName = "Orthopedics", Description = "Musculoskeletal problems and injuries.", Price = 300, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 8, ServiceName = "Physical Therapy", Description = "Rehabilitation and pain management.", Price = 300, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 9, ServiceName = "Podiatry", Description = "Foot and ankle care.", Price = 300, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 10, ServiceName = "ENT (Ear, Nose, and Throat)", Description = "Treatment of related conditions.", Price = 300, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 11, ServiceName = "Gynecology and Obstetrics", Description = "Women's health and prenatal care.", Price = 300, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 12, ServiceName = "Ophthalmology", Description = "Eye care.", Price = 300, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 13, ServiceName = "Dentistry", Description = "Oral health and dental procedures.", Price = 300, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 14, ServiceName = "Mental Health Services", Description = "Counseling, therapy, and psychiatric care.", Price = 200, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 15, ServiceName = "Addiction Services", Description = "Support for substance abuse and addiction recovery.", Price = 200, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) },
                    new Service { ServiceId = 16, ServiceName = "Laboratory and Diagnostic Services", Description = "Blood work, urine tests, imaging (X-rays, ultrasounds), and other diagnostic procedures.", Price = 200, CreatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 6, 23, 10, 0, 0, DateTimeKind.Utc) }
                );
            });

            // --- Vendor entity configuration (confirming existing setup) ---
            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.HasKey(e => e.VendorId).HasName("PK__Vendors__FC8653AD1D3DE96A");
                entity.Property(e => e.VendorId).HasColumnName("VendorID");
                entity.Property(e => e.VendorName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ContactPerson).HasMaxLength(100);
                entity.Property(e => e.ContactNumber).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            });

            // --- ClinicSetting entity configuration (confirming existing setup) ---
            modelBuilder.Entity<ClinicSetting>(entity =>
            {
                entity.Property(e => e.OpenTime).HasColumnType("time"); // Map TimeOnly to time SQL type
                entity.Property(e => e.CloseTime).HasColumnType("time");
                entity.Property(e => e.LunchStartTime).HasColumnType("time");
                entity.Property(e => e.LunchEndTime).HasColumnType("time");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                // NEW: Seed initial ClinicSetting data directly into the migration
                entity.HasData(
                    new ClinicSetting
                    {
                        Id = 1, // Important: provide a fixed ID for seeding
                        OpenTime = new TimeOnly(9, 0),    // 9:00 AM
                        CloseTime = new TimeOnly(17, 0),  // 5:00 PM
                        LunchStartTime = new TimeOnly(12, 0), // 12:00 PM
                        LunchEndTime = new TimeOnly(13, 0),   // 1:00 PM
                        CreatedAt = new DateTime(2025, 6, 24, 10, 0, 0, DateTimeKind.Utc) // Static DateTime
                    }
                );
            });

            // --- InventoryItem entity configuration (confirming existing setup) ---
            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.HasKey(e => e.ItemId).HasName("PK__Inventory__7BF7D5B66736C561");
                entity.Property(e => e.ItemId).HasColumnName("ItemID");
                entity.Property(e => e.ItemName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.UnitOfMeasure).HasMaxLength(50).IsRequired();
                entity.Property(e => e.VendorId).HasColumnName("VendorID");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(d => d.Vendor).WithMany(p => p.InventoryItems)
                    .HasForeignKey(d => d.VendorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Inventory__Vendo__6EF57B66");
            });

            // --- ItemBatch entity configuration (confirming existing setup) ---
            modelBuilder.Entity<ItemBatch>(entity =>
            {
                entity.HasKey(e => e.BatchId).HasName("PK__ItemBatc__B28D3F372095039A");
                entity.Property(e => e.BatchId).HasColumnName("BatchID");
                entity.Property(e => e.ItemId).HasColumnName("ItemID");
                entity.Property(e => e.BatchNumber).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.ExpirationDate).HasColumnType("date");
                entity.Property(e => e.CostPerUnit).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.SalePrice).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Item).WithMany(p => p.ItemBatches)
                    .HasForeignKey(d => d.ItemId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__ItemBatch__ItemI__72C60C4A");
            });

            // --- StockTransaction entity configuration (confirming existing setup) ---
            modelBuilder.Entity<StockTransaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId).HasName("PK__StockTra__55433B6A7F2D065C");
                entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
                entity.Property(e => e.BatchId).HasColumnName("BatchID");
                entity.Property(e => e.TransactionType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.TransactionDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(d => d.Batch).WithMany(p => p.StockTransactions)
                    .HasForeignKey(d => d.BatchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__StockTran__Batch__75A278FBC");
            });


            // VerificationCode configuration (keep as is)
            modelBuilder.Entity<VerificationCode>()
                   .HasQueryFilter(vc => vc.Patient != null && !vc.Patient.IsDeleted)
                   .HasOne(vc => vc.Patient)
                   .WithMany()
                   .HasForeignKey(vc => vc.PatientId)
                   .OnDelete(DeleteBehavior.Cascade);

            // --- Configure LabResult entity ---
            modelBuilder.Entity<LabResult>(entity =>
            {
                entity.HasKey(e => e.LabResultId);
                entity.Property(e => e.LabResultId).HasColumnName("LabResultID");

                entity.Property(e => e.TestName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ResultValue).HasMaxLength(255);
                entity.Property(e => e.Unit).HasMaxLength(50);
                entity.Property(e => e.ReferenceRange).HasMaxLength(100);
                entity.Property(e => e.Interpretation).HasMaxLength(500);
                entity.Property(e => e.ResultDate).HasColumnType("date").IsRequired(); // Store date only if time isn't critical

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                // Relationships for LabResult
                entity.HasOne(lr => lr.Patient)
                    .WithMany() // Or WithMany(p => p.LabResults) if you add ICollection<LabResult> to Patient
                    .HasForeignKey(lr => lr.PatientId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LabResults_Patients");

                entity.HasOne(lr => lr.MedicalRecord)
                    .WithMany() // Or WithMany(mr => mr.LabResults) if you add ICollection<LabResult> to MedicalRecord
                    .HasForeignKey(lr => lr.MedicalRecordId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_LabResults_MedicalRecords");

                entity.HasOne(lr => lr.Appointment)
                    .WithMany() // Or WithMany(a => a.LabResults) if you add ICollection<LabResult> to Appointment
                    .HasForeignKey(lr => lr.AppointmentId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_LabResults_Appointments");

                entity.HasOne(lr => lr.OrderedByStaff)
                    .WithMany() // Or WithMany(sd => sd.OrderedLabResults)
                    .HasForeignKey(lr => lr.OrderedByStaffId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_LabResults_StaffDetails");

                // Apply global query filter for soft delete
                entity.HasQueryFilter(lr => !lr.IsDeleted);
            });

            // --- Configure PatientDocument entity ---
            modelBuilder.Entity<PatientDocument>(entity =>
            {
                entity.HasKey(e => e.DocumentId);
                entity.Property(e => e.DocumentId).HasColumnName("DocumentID");

                entity.Property(e => e.DocumentName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.DocumentType).HasMaxLength(50);
                entity.Property(e => e.FilePathOrUrl).IsRequired().HasMaxLength(500); // Path or URL to file
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.UploadDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime"); // When it was uploaded

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                // Relationships for PatientDocument
                entity.HasOne(pd => pd.Patient)
                    .WithMany() // Or WithMany(p => p.PatientDocuments) if you add ICollection<PatientDocument> to Patient
                    .HasForeignKey(pd => pd.PatientId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_PatientDocuments_Patients");

                entity.HasOne(pd => pd.UploadedByStaff)
                    .WithMany() // Or WithMany(sd => sd.UploadedDocuments)
                    .HasForeignKey(pd => pd.UploadedByStaffId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_PatientDocuments_StaffDetails");

                // Apply global query filter for soft delete
                entity.HasQueryFilter(pd => !pd.IsDeleted);
            });

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}