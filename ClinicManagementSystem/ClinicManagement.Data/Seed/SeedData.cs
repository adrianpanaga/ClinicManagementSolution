// Seed/SeedData.cs
using ClinicManagement.Data.Context;
using ClinicManagement.Data.Models;
using ClinicManagement.Data.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagement.Data.Seed
{
    public static class SeedData
    {
        public static async Task SeedUsersAndRoles(UserManager<User> userManager, RoleManager<Role> roleManager, ClinicManagementDbContext context)
        {
            // --- Seed Roles (from your provided OnModelCreating HasData) ---
            var roles = new List<Role>
            {
                new Role { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new Role { Id = 2, Name = "Receptionist", NormalizedName = "RECEPTIONIST" },
                new Role { Id = 3, Name = "HR", NormalizedName = "HR" },
                new Role { Id = 4, Name = "Doctor", NormalizedName = "DOCTOR" },
                new Role { Id = 5, Name = "Nurse", NormalizedName = "NURSE" },
                // new Role { Id = 6, Name = "Patient", NormalizedName = "PATIENT" }, // Only include if Patient roles are actively used in Identity
                new Role { Id = 7, Name = "InventoryManager", NormalizedName = "INVENTORYMANAGER" }
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Name)) // CS8604: Possible null reference argument - This warning can often be ignored for string literals
                {
                    await roleManager.CreateAsync(role);
                }
            }
            await context.SaveChangesAsync();

            // --- Seed Users (for Staff Portal logins) ---
            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                adminUser = new User { UserName = "admin", Email = "admin@clinic.com", EmailConfirmed = true, CreatedAt = DateTime.UtcNow, IsActive = true };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded) await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            var drAvaChenUser = await userManager.FindByNameAsync("ava.chen");
            if (drAvaChenUser == null)
            {
                drAvaChenUser = new User { UserName = "ava.chen", Email = "ava.chen@clinic.com", EmailConfirmed = true, CreatedAt = DateTime.UtcNow, IsActive = true };
                var result = await userManager.CreateAsync(drAvaChenUser, "Doctor@123");
                if (result.Succeeded) await userManager.AddToRoleAsync(drAvaChenUser, "Doctor");
            }

            var drBenRobertsUser = await userManager.FindByNameAsync("ben.roberts");
            if (drBenRobertsUser == null)
            {
                drBenRobertsUser = new User { UserName = "ben.roberts", Email = "ben.roberts@clinic.com", EmailConfirmed = true, CreatedAt = DateTime.UtcNow, IsActive = true };
                var result = await userManager.CreateAsync(drBenRobertsUser, "Doctor@123");
                if (result.Succeeded) await userManager.AddToRoleAsync(drBenRobertsUser, "Doctor");
            }

            var receptionistUser = await userManager.FindByNameAsync("receptionist");
            if (receptionistUser == null)
            {
                receptionistUser = new User { UserName = "receptionist", Email = "receptionist@clinic.com", EmailConfirmed = true, CreatedAt = DateTime.UtcNow, IsActive = true };
                var result = await userManager.CreateAsync(receptionistUser, "Receptionist@123");
                if (result.Succeeded) await userManager.AddToRoleAsync(receptionistUser, "Receptionist");
            }

            await context.SaveChangesAsync();


            // --- Link StaffDetails (already existing via HasData) to Users ---
            var avaChenStaff = await context.StaffDetails.FirstOrDefaultAsync(s => s.StaffId == 101);
            if (avaChenStaff != null && drAvaChenUser != null && avaChenStaff.UserId != drAvaChenUser.Id)
            {
                avaChenStaff.UserId = drAvaChenUser.Id;
                context.StaffDetails.Update(avaChenStaff);
            }

            var benRobertsStaff = await context.StaffDetails.FirstOrDefaultAsync(s => s.StaffId == 102);
            if (benRobertsStaff != null && drBenRobertsUser != null && benRobertsStaff.UserId != drBenRobertsUser.Id)
            {
                benRobertsStaff.UserId = drBenRobertsUser.Id;
                context.StaffDetails.Update(benRobertsStaff);
            }
            await context.SaveChangesAsync();


            // --- Seed Patients (if not already in HasData) ---
            var johnDoePatientData = await context.Patients.FirstOrDefaultAsync(p => p.PatientId == 1);
            if (johnDoePatientData == null)
            {
                johnDoePatientData = new Patient
                {
                    PatientId = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    ContactNumber = "(123) 456-7890",
                    Email = "johndoe@example.com",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                context.Patients.Add(johnDoePatientData);
            }
            else
            {
                if (johnDoePatientData.UserId != null)
                {
                    johnDoePatientData.UserId = null;
                    context.Patients.Update(johnDoePatientData);
                }
            }

            var raiPatientData = await context.Patients.FirstOrDefaultAsync(p => p.PatientId == 2);
            if (raiPatientData == null)
            {
                raiPatientData = new Patient
                {
                    PatientId = 2,
                    FirstName = "Rai",
                    LastName = "Aaden Paul Panaga",
                    ContactNumber = "(142) 678-9012",
                    Email = "rai@example.com",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                context.Patients.Add(raiPatientData);
            }
            else
            {
                if (raiPatientData.UserId != null)
                {
                    raiPatientData.UserId = null;
                    context.Patients.Update(raiPatientData);
                }
            }
            await context.SaveChangesAsync();


            // --- Fetch Services (from your provided OnModelCreating HasData) ---
            var generalConsultationService = await context.Services.FirstOrDefaultAsync(s => s.ServiceId == 1);
            var vaccinationsService = await context.Services.FirstOrDefaultAsync(s => s.ServiceId == 3);
            var ophthalmologyService = await context.Services.FirstOrDefaultAsync(s => s.ServiceId == 12);
            var commonIllnessesService = await context.Services.FirstOrDefaultAsync(s => s.ServiceId == 2);

            // Fetch actual StaffDetail records (doctors) for appointments/medical records
            // CS8602, CS8602 warnings are likely from these (e.g., if .FirstOrDefaultAsync() returns null)
            // Use null-forgiving operator (!) if you're sure they won't be null based on prior seeding/HasData
            var doctorAvaChen = await context.StaffDetails.FirstOrDefaultAsync(s => s.StaffId == 101); // CS8602 here
            var doctorJackDavis = await context.StaffDetails.FirstOrDefaultAsync(s => s.StaffId == 110);


            // --- Seed Appointments (from your provided SELECT Appointments query) ---
            // Ensure you are using the correct StaffId (e.g., 101 for Ava Chen) and ServiceId from your tables.
            var apptJohn1 = await context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == 1);
            if (apptJohn1 == null && johnDoePatientData != null && doctorAvaChen != null && generalConsultationService != null)
            {
                apptJohn1 = new Appointment
                {
                    AppointmentId = 1,
                    PatientId = johnDoePatientData.PatientId,
                    ServiceId = generalConsultationService.ServiceId,
                    DoctorId = doctorAvaChen.StaffId,
                    AppointmentDateTime = new DateTime(2025, 06, 29, 01, 00, 00),
                    Notes = "Annual check-up",
                    Status = AppointmentStatus.Scheduled,
                    CreatedAt = new DateTime(2025, 06, 30, 04, 13, 19).ToUniversalTime()
                };
                context.Appointments.Add(apptJohn1);
            }

            var apptJohn10 = await context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == 10);
            if (apptJohn10 == null && johnDoePatientData != null && doctorAvaChen != null && generalConsultationService != null)
            {
                apptJohn10 = new Appointment
                {
                    AppointmentId = 10,
                    PatientId = johnDoePatientData.PatientId,
                    ServiceId = generalConsultationService.ServiceId,
                    DoctorId = doctorAvaChen.StaffId,
                    AppointmentDateTime = new DateTime(2025, 07, 09, 03, 30, 00),
                    Notes = "Cough",
                    Status = AppointmentStatus.Scheduled,
                    CreatedAt = new DateTime(2025, 07, 05, 06, 27, 59).ToUniversalTime()
                };
                context.Appointments.Add(apptJohn10);
            }

            var apptRai14 = await context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == 14);
            if (apptRai14 == null && raiPatientData != null && doctorAvaChen != null && vaccinationsService != null)
            {
                apptRai14 = new Appointment
                {
                    AppointmentId = 14,
                    PatientId = raiPatientData.PatientId,
                    ServiceId = vaccinationsService.ServiceId,
                    DoctorId = doctorAvaChen.StaffId,
                    AppointmentDateTime = new DateTime(2025, 07, 05, 01, 00, 00),
                    Notes = "vaccine",
                    Status = AppointmentStatus.Scheduled,
                    CreatedAt = new DateTime(2025, 07, 04, 23, 41, 43).ToUniversalTime()
                };
                context.Appointments.Add(apptRai14);
            }

            var apptRai17 = await context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == 17);
            if (apptRai17 == null && raiPatientData != null && doctorJackDavis != null && ophthalmologyService != null)
            {
                apptRai17 = new Appointment
                {
                    AppointmentId = 17,
                    PatientId = raiPatientData.PatientId,
                    ServiceId = ophthalmologyService.ServiceId,
                    DoctorId = doctorJackDavis.StaffId,
                    AppointmentDateTime = new DateTime(2025, 07, 05, 01, 00, 00),
                    Notes = "Eye care",
                    Status = AppointmentStatus.Scheduled,
                    CreatedAt = new DateTime(2025, 07, 05, 19, 28, 35).ToUniversalTime()
                };
                context.Appointments.Add(apptRai17);
            }
            // Add other existing appointments from your SELECT query if needed for completeness, ensure IDs are matched.
            var apptJohn2 = await context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == 2); // For Medical Record seeding if needed

            await context.SaveChangesAsync();


            // --- Seed Medical Records ---
            // Get existing appointments by ID for linking
            // Need to re-fetch if they were added in this seeding process and not from HasData
            apptJohn1 = await context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == 1);
            apptJohn10 = await context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == 10);
            apptRai14 = await context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == 14);
            apptRai17 = await context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == 17);

            // Fetch Medical Records
            // CRITICAL FIX FOR CS4010: Ensure you are comparing actual DateTime objects.
            // Avoid async operations inside FirstOrDefaultAsync predicates if possible.
            // Instead, query by PatientId and then filter in memory if necessary, or simplify the predicate.

            // Example of how to fix CS4010 if you still get it for MedicalRecord:
            // Instead of: var mr1_john = await context.MedicalRecords.FirstOrDefaultAsync(mr => mr.PatientId == johnDoePatientData.PatientId && mr.AppointmentId == (await context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == 1))?.AppointmentId);
            // Do:

            // --- Medical Record for John Doe (linked to apptJohn1) ---
            if (johnDoePatientData != null && doctorAvaChen != null && generalConsultationService != null && apptJohn1 != null)
            {
                // Check if Medical Record exists by patient, appointment, staff, service and created date (simplified)
                var mr1_john = await context.MedicalRecords.FirstOrDefaultAsync(mr =>
                    mr.PatientId == johnDoePatientData.PatientId &&
                    mr.AppointmentId == apptJohn1.AppointmentId &&
                    mr.StaffId == doctorAvaChen.StaffId &&
                    mr.ServiceId == generalConsultationService.ServiceId &&
                    mr.CreatedAt.Date == apptJohn1.AppointmentDateTime.AddMinutes(30).Date
                );

                if (mr1_john == null)
                {
                    context.MedicalRecords.Add(new MedicalRecord
                    {
                        PatientId = johnDoePatientData.PatientId,
                        AppointmentId = apptJohn1.AppointmentId,
                        StaffId = doctorAvaChen.StaffId,
                        ServiceId = generalConsultationService.ServiceId,
                        Diagnosis = "Routine Check-up, Healthy",
                        Treatment = "Advised continuation of healthy lifestyle. No specific treatment needed.",
                        Prescription = "None",
                        CreatedAt = apptJohn1.AppointmentDateTime.AddMinutes(30),
                        IsDeleted = false
                    });
                }
            }

            // --- Medical Record for John Doe (linked to apptJohn10) ---
            if (johnDoePatientData != null && doctorAvaChen != null && generalConsultationService != null && apptJohn10 != null)
            {
                var mr2_john = await context.MedicalRecords.FirstOrDefaultAsync(mr =>
                    mr.PatientId == johnDoePatientData.PatientId &&
                    mr.AppointmentId == apptJohn10.AppointmentId &&
                    mr.StaffId == doctorAvaChen.StaffId &&
                    mr.ServiceId == generalConsultationService.ServiceId &&
                    mr.CreatedAt.Date == apptJohn10.AppointmentDateTime.AddMinutes(30).Date
                );
                if (mr2_john == null)
                {
                    context.MedicalRecords.Add(new MedicalRecord
                    {
                        PatientId = johnDoePatientData.PatientId,
                        AppointmentId = apptJohn10.AppointmentId,
                        StaffId = doctorAvaChen.StaffId,
                        ServiceId = generalConsultationService.ServiceId,
                        Diagnosis = "Acute Bronchitis",
                        Treatment = "Prescribed antibiotics and cough syrup.",
                        Prescription = "Amoxicillin 500mg (3x daily for 7 days), Dextromethorphan (as needed for cough)",
                        CreatedAt = apptJohn10.AppointmentDateTime.AddMinutes(30),
                        IsDeleted = false
                    });
                }
            }

            // --- Medical Record for Rai (linked to apptRai14) ---
            if (raiPatientData != null && doctorAvaChen != null && vaccinationsService != null && apptRai14 != null)
            {
                var mr1_rai = await context.MedicalRecords.FirstOrDefaultAsync(mr =>
                    mr.PatientId == raiPatientData.PatientId &&
                    mr.AppointmentId == apptRai14.AppointmentId &&
                    mr.StaffId == doctorAvaChen.StaffId &&
                    mr.ServiceId == vaccinationsService.ServiceId &&
                    mr.CreatedAt.Date == apptRai14.AppointmentDateTime.AddMinutes(30).Date
                );
                if (mr1_rai == null)
                {
                    context.MedicalRecords.Add(new MedicalRecord
                    {
                        PatientId = raiPatientData.PatientId,
                        AppointmentId = apptRai14.AppointmentId,
                        StaffId = doctorAvaChen.StaffId,
                        ServiceId = vaccinationsService.ServiceId,
                        Diagnosis = "Vaccination Administration",
                        Treatment = "Flu shot administered.",
                        Prescription = "None",
                        CreatedAt = apptRai14.AppointmentDateTime.AddMinutes(30),
                        IsDeleted = false
                    });
                }
            }

            // --- Medical Record for Rai (linked to apptRai17) ---
            if (raiPatientData != null && doctorJackDavis != null && ophthalmologyService != null && apptRai17 != null)
            {
                var mr2_rai = await context.MedicalRecords.FirstOrDefaultAsync(mr =>
                    mr.PatientId == raiPatientData.PatientId &&
                    mr.AppointmentId == apptRai17.AppointmentId &&
                    mr.StaffId == doctorJackDavis.StaffId &&
                    mr.ServiceId == ophthalmologyService.ServiceId &&
                    mr.CreatedAt.Date == apptRai17.AppointmentDateTime.AddMinutes(30).Date
                );
                if (mr2_rai == null)
                {
                    context.MedicalRecords.Add(new MedicalRecord
                    {
                        PatientId = raiPatientData.PatientId,
                        AppointmentId = apptRai17.AppointmentId,
                        StaffId = doctorJackDavis.StaffId,
                        ServiceId = ophthalmologyService.ServiceId,
                        Diagnosis = "Minor Eye Irritation",
                        Treatment = "Eye drops prescribed.",
                        Prescription = "Artificial tears, use 2 drops in each eye 3 times a day.",
                        CreatedAt = apptRai17.AppointmentDateTime.AddMinutes(30),
                        IsDeleted = false
                    });
                }
            }
            await context.SaveChangesAsync();


            // --- Seed Lab Results ---
            // Ensure ResultDate is DateTime, not DateOnly, when creating the model directly.
            if (johnDoePatientData != null && doctorAvaChen != null)
            {
                if (apptJohn1 != null)
                {
                    // Lab result linked to a medical record and appointment
                    // Find the medical record *before* checking LabResult
                    var mr1_john_fetched = await context.MedicalRecords.FirstOrDefaultAsync(mr =>
                        mr.PatientId == johnDoePatientData.PatientId &&
                        mr.AppointmentId == apptJohn1.AppointmentId &&
                        mr.StaffId == doctorAvaChen.StaffId &&
                        mr.ServiceId == generalConsultationService.ServiceId &&
                        mr.CreatedAt.Date == apptJohn1.AppointmentDateTime.AddMinutes(30).Date
                    );

                    var labResult1_john = await context.LabResults.FirstOrDefaultAsync(lr =>
                        lr.PatientId == johnDoePatientData.PatientId &&
                        lr.TestName == "Complete Blood Count" &&
                        lr.ResultDate.Date == apptJohn1.AppointmentDateTime.AddDays(1).Date // Compare DateTimes
                    );
                    if (labResult1_john == null)
                    {
                        context.LabResults.Add(new LabResult
                        {
                            PatientId = johnDoePatientData.PatientId,
                            MedicalRecordId = mr1_john_fetched?.RecordId, // Use fetched medical record ID
                            AppointmentId = apptJohn1.AppointmentId,
                            TestName = "Complete Blood Count",
                            ResultValue = "Normal",
                            Unit = "",
                            ReferenceRange = "Normal Range",
                            Interpretation = "All parameters within normal limits.",
                            ResultDate = apptJohn1.AppointmentDateTime.AddDays(1), // Assign DateTime directly
                            OrderedByStaffId = doctorAvaChen.StaffId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                // Lab result not linked to a specific medical record or appointment (e.g. standalone test)
                var testDateForLab2 = DateTime.Today.AddDays(-7).Date;
                var labResult2_john = await context.LabResults.FirstOrDefaultAsync(lr =>
                    lr.PatientId == johnDoePatientData.PatientId &&
                    lr.TestName == "Blood Sugar (Fasting)" &&
                    lr.ResultDate.Date == testDateForLab2 // Compare DateTimes
                );
                if (labResult2_john == null)
                {
                    context.LabResults.Add(new LabResult
                    {
                        PatientId = johnDoePatientData.PatientId,
                        TestName = "Blood Sugar (Fasting)",
                        ResultValue = "95",
                        Unit = "mg/dL",
                        ReferenceRange = "70-99",
                        Interpretation = "Slightly elevated, advise diet review.",
                        ResultDate = testDateForLab2, // Assign DateTime directly
                        OrderedByStaffId = doctorAvaChen.StaffId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (raiPatientData != null && doctorAvaChen != null && apptRai17 != null)
            {
                var testDateForLabRai1 = apptRai17.AppointmentDateTime.AddDays(2).Date;
                var labResult1_rai = await context.LabResults.FirstOrDefaultAsync(lr =>
                    lr.PatientId == raiPatientData.PatientId &&
                    lr.TestName == "Urinalysis" &&
                    lr.ResultDate.Date == testDateForLabRai1 // Compare DateTimes
                );
                if (labResult1_rai == null)
                {
                    context.LabResults.Add(new LabResult
                    {
                        PatientId = raiPatientData.PatientId,
                        AppointmentId = apptRai17.AppointmentId,
                        TestName = "Urinalysis",
                        ResultValue = "Clear, Negative",
                        Unit = "",
                        ReferenceRange = "Normal",
                        Interpretation = "No abnormalities detected.",
                        ResultDate = testDateForLabRai1, // Assign DateTime directly
                        OrderedByStaffId = doctorAvaChen.StaffId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await context.SaveChangesAsync();


            // --- Seed Triage Records ---
            // These will be inserted after all other data, which they might depend on.
            if (johnDoePatientData != null && apptJohn1 != null)
            {
                var triage1_john = await context.TriageRecords.FirstOrDefaultAsync(tr => tr.PatientId == johnDoePatientData.PatientId && tr.AppointmentId == apptJohn1.AppointmentId);
                if (triage1_john == null)
                {
                    context.TriageRecords.Add(new TriageRecord
                    {
                        PatientId = johnDoePatientData.PatientId,
                        AppointmentId = apptJohn1.AppointmentId,
                        ChiefComplaint = "Mild headache and fatigue.",
                        Temperature = 37.2m,
                        BloodPressureSystolic = 120,
                        BloodPressureDiastolic = 80,
                        PulseRate = 75,
                        RespiratoryRate = 18,
                        Weight = 70.1m,
                        Height = 175.0m,
                        Notes = "Patient reports feeling tired for 3 days. No other significant symptoms.",
                        CreatedAt = apptJohn1.AppointmentDateTime.AddMinutes(15)
                    });
                }
            }

            if (raiPatientData != null && apptRai17 != null)
            {
                var triage1_rai = await context.TriageRecords.FirstOrDefaultAsync(tr => tr.PatientId == raiPatientData.PatientId && tr.AppointmentId == apptRai17.AppointmentId);
                if (triage1_rai == null)
                {
                    context.TriageRecords.Add(new TriageRecord
                    {
                        PatientId = raiPatientData.PatientId,
                        AppointmentId = apptRai17.AppointmentId,
                        ChiefComplaint = "Eye discomfort and redness.",
                        Temperature = 36.8m,
                        BloodPressureSystolic = 118,
                        BloodPressureDiastolic = 78,
                        PulseRate = 70,
                        RespiratoryRate = 16,
                        Weight = 65.5m,
                        Height = 160.0m,
                        Notes = "Right eye appears slightly irritated. No discharge reported.",
                        CreatedAt = apptRai17.AppointmentDateTime.AddMinutes(10)
                    });
                }
            }

            // A triage record without a direct appointment link
            if (johnDoePatientData != null)
            {
                var triage2_john = await context.TriageRecords.FirstOrDefaultAsync(tr => tr.PatientId == johnDoePatientData.PatientId && tr.AppointmentId == null && tr.CreatedAt.Date == DateTime.Today.AddDays(-2).Date);
                if (triage2_john == null)
                {
                    context.TriageRecords.Add(new TriageRecord
                    {
                        PatientId = johnDoePatientData.PatientId,
                        AppointmentId = null,
                        ChiefComplaint = "Sudden onset of mild fever.",
                        Temperature = 38.1m,
                        BloodPressureSystolic = 125,
                        BloodPressureDiastolic = 82,
                        PulseRate = 80,
                        RespiratoryRate = 19,
                        Weight = 70.3m,
                        Height = 175.0m,
                        Notes = "Patient came for walk-in triage. Advised to book a full consultation.",
                        CreatedAt = DateTime.Today.AddDays(-2).AddHours(11)
                    });
                }
            }

            await context.SaveChangesAsync(); // Final save for all newly seeded data (including TriageRecords)

            // --- NEW: Seed Patient Documents ---
            var receptionistStaff = await context.StaffDetails.FirstOrDefaultAsync(s => s.JobTitle == "Receptionist"); // Assuming you have a receptionist staff member

            if (johnDoePatientData != null && receptionistStaff != null)
            {
                var doc1_john = await context.PatientDocuments.FirstOrDefaultAsync(pd => pd.PatientId == johnDoePatientData.PatientId && pd.DocumentName == "Consent Form - Annual Checkup 2025");
                if (doc1_john == null)
                {
                    context.PatientDocuments.Add(new PatientDocument
                    {
                        PatientId = johnDoePatientData.PatientId,
                        DocumentName = "Consent Form - Annual Checkup 2025",
                        DocumentType = "Consent Form",
                        FilePathOrUrl = "https://clinicfiles.com/johndoe/consent_2025_annual.pdf", // Placeholder URL
                        Notes = "Signed consent for 2025 annual physical.",
                        UploadedByStaffId = receptionistStaff.StaffId,
                        UploadDate = new DateTime(2025, 1, 10),
                        CreatedAt = DateTime.UtcNow
                    });
                }

                var doc2_john = await context.PatientDocuments.FirstOrDefaultAsync(pd => pd.PatientId == johnDoePatientData.PatientId && pd.DocumentName == "Referral Letter - Cardiology");
                if (doc2_john == null)
                {
                    context.PatientDocuments.Add(new PatientDocument
                    {
                        PatientId = johnDoePatientData.PatientId,
                        DocumentName = "Referral Letter - Cardiology",
                        DocumentType = "Referral Letter",
                        FilePathOrUrl = "https://clinicfiles.com/johndoe/referral_cardiology.pdf", // Placeholder URL
                        Notes = "Referral to Dr. Clara Garcia for cardiac evaluation.",
                        UploadedByStaffId = receptionistStaff.StaffId,
                        UploadDate = new DateTime(2025, 2, 15),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (raiPatientData != null && receptionistStaff != null)
            {
                var doc1_rai = await context.PatientDocuments.FirstOrDefaultAsync(pd => pd.PatientId == raiPatientData.PatientId && pd.DocumentName == "Insurance Policy - HealthSure");
                if (doc1_rai == null)
                {
                    context.PatientDocuments.Add(new PatientDocument
                    {
                        PatientId = raiPatientData.PatientId,
                        DocumentName = "Insurance Policy - HealthSure",
                        DocumentType = "Insurance Policy",
                        FilePathOrUrl = "https://clinicfiles.com/raipanaga/insurance_healthsure.pdf", // Placeholder URL
                        Notes = "Copy of patient's current health insurance policy.",
                        UploadedByStaffId = receptionistStaff.StaffId,
                        UploadDate = new DateTime(2024, 11, 5),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await context.SaveChangesAsync();

            // ... (Final save for all seeded data) ...
            Console.WriteLine("Database seeding complete.");
        }

    }
}