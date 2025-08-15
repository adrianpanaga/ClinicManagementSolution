// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\Controllers\MedicalRecordsController.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // For authorization attributes
using System.Security.Claims; // Required for accessing user claims (e.g., UserId)

using ClinicManagement.Data.Context; // Your DbContext
using ClinicManagement.Data.Models;   // Your EF Core models
using ClinicManagement.Api.DTOs.MedicalRecords; // Your MedicalRecords DTOs
using ClinicManagement.Api.DTOs.Appointments;   // For AppointmentDto
using ClinicManagement.Api.DTOs.Patients;      // For PatientDetailsDto
using ClinicManagement.Api.DTOs.StaffDetails;  // For StaffDetailDto
using ClinicManagement.Api.DTOs.Services;      // For ServiceDto


namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All actions in this controller require an authenticated user by default
    public class MedicalRecordsController : ControllerBase
    {
        private readonly ClinicManagementDbContext _context;

        public MedicalRecordsController(ClinicManagementDbContext context)
        {
            _context = context;
        }

        // Helper method to manually map MedicalRecord model to MedicalRecordsDto
        private MedicalRecordsDto MapToMedicalRecordsDto(MedicalRecord medicalRecord)
        {
            if (medicalRecord == null) return null;

            return new MedicalRecordsDto
            {
                RecordId = medicalRecord.RecordId,
                PatientId = medicalRecord.PatientId,
                AppointmentId = medicalRecord.AppointmentId,
                StaffId = medicalRecord.StaffId,
                ServiceId = medicalRecord.ServiceId,
                Diagnosis = medicalRecord.Diagnosis,
                Treatment = medicalRecord.Treatment,
                Prescription = medicalRecord.Prescription,
                CreatedAt = medicalRecord.CreatedAt,
                UpdatedAt = medicalRecord.UpdatedAt,
                IsDeleted = medicalRecord.IsDeleted, // Include soft delete status

                // Manually map related entities to their respective DTOs
                Patient = medicalRecord.Patient != null ? new PatientDetailsDto
                {
                    PatientId = medicalRecord.Patient.PatientId,
                    FirstName = medicalRecord.Patient.FirstName,
                    MiddleName = medicalRecord.Patient.MiddleName,
                    LastName = medicalRecord.Patient.LastName,
                    Gender = medicalRecord.Patient.Gender,
                    DateOfBirth = medicalRecord.Patient.DateOfBirth.HasValue ? DateOnly.FromDateTime(medicalRecord.Patient.DateOfBirth.Value) : (DateOnly?)null, // Map DateTime? to DateOnly?
                    Address = medicalRecord.Patient.Address,
                    ContactNumber = medicalRecord.Patient.ContactNumber,
                    Email = medicalRecord.Patient.Email,
                    BloodType = medicalRecord.Patient.BloodType,
                    EmergencyContactName = medicalRecord.Patient.EmergencyContactName,
                    EmergencyContactNumber = medicalRecord.Patient.EmergencyContactNumber,
                    PhotoUrl = medicalRecord.Patient.PhotoUrl,
                    CreatedAt = medicalRecord.Patient.CreatedAt,
                    UpdatedAt = medicalRecord.Patient.UpdatedAt,
                    UserId = medicalRecord.Patient.UserId,
                    IsDeleted = medicalRecord.Patient.IsDeleted
                } : null,
                Staff = medicalRecord.Staff != null ? new StaffDetailDto
                {
                    StaffId = medicalRecord.Staff.StaffId,
                    FirstName = medicalRecord.Staff.FirstName,
                    MiddleName = medicalRecord.Staff.MiddleName,
                    LastName = medicalRecord.Staff.LastName,
                    JobTitle = medicalRecord.Staff.JobTitle,
                    Specialization = medicalRecord.Staff.Specialization,
                    ContactNumber = medicalRecord.Staff.ContactNumber,
                    Email = medicalRecord.Staff.Email,
                    CreatedAt = medicalRecord.Staff.CreatedAt,
                    UpdatedAt = medicalRecord.Staff.UpdatedAt,
                    UserId = medicalRecord.Staff.UserId,
                    IsDeleted = medicalRecord.Staff.IsDeleted
                } : null,
                Appointment = medicalRecord.Appointment != null ? new AppointmentDto
                {
                    AppointmentId = medicalRecord.Appointment.AppointmentId,
                    PatientId = medicalRecord.Appointment.PatientId,
                    DoctorId = medicalRecord.Appointment.DoctorId,
                    ServiceId = medicalRecord.Appointment.ServiceId, // ServiceId remains non-nullable
                    AppointmentDateTime = medicalRecord.Appointment.AppointmentDateTime,
                    Status = medicalRecord.Appointment.Status,
                    Notes = medicalRecord.Appointment.Notes,
                    CreatedAt = medicalRecord.Appointment.CreatedAt,
                    UpdatedAt = medicalRecord.Appointment.UpdatedAt
                } : null,
                Service = medicalRecord.Service != null ? new ServiceDto // Ensure ServiceDto exists
                {
                    ServiceId = medicalRecord.Service.ServiceId,
                    ServiceName = medicalRecord.Service.ServiceName,
                    Description = medicalRecord.Service.Description,
                    Price = medicalRecord.Service.Price,
                    CreatedAt = medicalRecord.Service.CreatedAt,
                    UpdatedAt = medicalRecord.Service.UpdatedAt
                } : null
            };
        }

        // GET: api/MedicalRecords
        /// <summary>
        /// Retrieves all medical records. For Admins/HR, can optionally include deleted records.
        /// Requires Admin or HR role to see all records, otherwise limited by user role.
        /// </summary>
        /// <param name="includeDeleted">Optional. If true and user is Admin/HR, includes soft-deleted records.</param>
        /// <returns>A list of MedicalRecordsDto.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Doctor,Nurse,Patient")] // Broad access, then filter
        public async Task<ActionResult<IEnumerable<MedicalRecordsDto>>> GetMedicalRecords([FromQuery] bool includeDeleted = false)
        {
            if (_context.MedicalRecords == null)
            {
                return NotFound();
            }

            IQueryable<MedicalRecord> query = _context.MedicalRecords;

            // Apply global query filter for IsDeleted by default.
            // Bypass if includeDeleted is true AND the user is Admin or HR.
            if (includeDeleted && (User.IsInRole("Admin") || User.IsInRole("HR")))
            {
                query = query.IgnoreQueryFilters();
            }

            // Always include related entities for DTO mapping
            query = query
                .Include(mr => mr.Patient)
                .Include(mr => mr.Staff)
                .Include(mr => mr.Appointment)
                .Include(mr => mr.Service);

            var medicalRecords = await query.ToListAsync();

            // Apply specific authorization filters based on role if not Admin/HR
            if (!(User.IsInRole("Admin") || User.IsInRole("HR")))
            {
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized("Could not identify current user.");
                }

                if (User.IsInRole("Patient"))
                {
                    var patientIdForCurrentUser = await _context.Patients
                                                                  .Where(p => p.UserId == currentUserId)
                                                                  .Select(p => p.PatientId)
                                                                  .FirstOrDefaultAsync();
                    medicalRecords = medicalRecords.Where(mr => mr.PatientId == patientIdForCurrentUser).ToList();
                }
                else if (User.IsInRole("Doctor") || User.IsInRole("Nurse"))
                {
                    var staffIdForCurrentUser = await _context.StaffDetails
                                                                .Where(sd => sd.UserId == currentUserId)
                                                                .Select(sd => sd.StaffId)
                                                                .FirstOrDefaultAsync();
                    medicalRecords = medicalRecords.Where(mr => mr.StaffId == staffIdForCurrentUser).ToList();
                }
                else // For any other unexpected roles, or if no specific filter applies, deny
                {
                    return Forbid("You do not have sufficient permissions to view medical records.");
                }
            }


            return medicalRecords.Select(mr => MapToMedicalRecordsDto(mr)).ToList();
        }

        // GET: api/MedicalRecords/5
        /// <summary>
        /// Retrieves a specific medical record by its ID.
        /// Requires Admin, Doctor, Nurse, or the Patient linked to the record.
        /// Admins/HR can retrieve deleted records by ID.
        /// </summary>
        /// <param name="id">The ID of the medical record.</param>
        /// <returns>The MedicalRecordsDto if found, otherwise NotFound.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,HR,Doctor,Nurse,Patient")]
        public async Task<ActionResult<MedicalRecordsDto>> GetMedicalRecord(int id)
        {
            if (_context.MedicalRecords == null)
            {
                return NotFound();
            }

            IQueryable<MedicalRecord> query = _context.MedicalRecords;

            // Admins/HR can retrieve deleted records by ID
            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                query = query.IgnoreQueryFilters();
            }

            var medicalRecord = await query
                                      .Include(mr => mr.Patient)
                                      .Include(mr => mr.Staff)
                                      .Include(mr => mr.Appointment)
                                      .Include(mr => mr.Service)
                                      .FirstOrDefaultAsync(mr => mr.RecordId == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            // Custom authorization for Patients, Doctors, Nurses:
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            {
                // This shouldn't happen if [Authorize] is working, but defensive check
                return Unauthorized("Could not identify current user for specific access checks.");
            }

            if (User.IsInRole("Patient"))
            {
                var patientLinkedToRecord = await _context.Patients
                                                            .Where(p => p.PatientId == medicalRecord.PatientId)
                                                            .Select(p => p.UserId)
                                                            .FirstOrDefaultAsync();

                if (patientLinkedToRecord == null || patientLinkedToRecord != currentUserId)
                {
                    return Forbid("You do not have access to this medical record.");
                }
            }
            else if (User.IsInRole("Doctor") || User.IsInRole("Nurse"))
            {
                var staffIdForCurrentUser = await _context.StaffDetails
                                                            .Where(sd => sd.UserId == currentUserId)
                                                            .Select(sd => sd.StaffId)
                                                            .FirstOrDefaultAsync();
                if (staffIdForCurrentUser == 0 || medicalRecord.StaffId != staffIdForCurrentUser)
                {
                    return Forbid("You do not have access to this medical record as it's not associated with your staff ID.");
                }
            }


            return MapToMedicalRecordsDto(medicalRecord);
        }

        // GET: api/MedicalRecords/patient/{patientId}
        /// <summary>
        /// Retrieves all medical records for a specific patient.
        /// Requires Admin, Doctor, Nurse, or the specific Patient.
        /// Admins/HR can optionally include deleted records for a patient.
        /// </summary>
        /// <param name="patientId">The ID of the patient.</param>
        /// <param name="includeDeleted">Optional. If true and user is Admin/HR, includes soft-deleted records.</param>
        /// <returns>A list of MedicalRecordsDto for the specified patient.</returns>
        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Admin,HR,Doctor,Nurse,Patient")]
        public async Task<ActionResult<IEnumerable<MedicalRecordsDto>>> GetMedicalRecordsByPatient(int patientId, [FromQuery] bool includeDeleted = false)
        {
            // Custom authorization logic for Patients:
            if (User.IsInRole("Patient"))
            {
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized("Could not identify current user.");
                }

                var patientIdForCurrentUser = await _context.Patients
                                                              .Where(p => p.UserId == currentUserId)
                                                              .Select(p => p.PatientId)
                                                              .FirstOrDefaultAsync();

                if (patientIdForCurrentUser == 0 || patientIdForCurrentUser != patientId)
                {
                    return Forbid("You can only view your own medical records.");
                }
            }

            if (_context.MedicalRecords == null)
            {
                return NotFound();
            }

            IQueryable<MedicalRecord> query = _context.MedicalRecords;

            // Admins/HR can retrieve deleted records for a specific patient
            if (includeDeleted && (User.IsInRole("Admin") || User.IsInRole("HR")))
            {
                query = query.IgnoreQueryFilters();
            }

            var medicalRecords = await query
                                 .Where(mr => mr.PatientId == patientId)
                                 .Include(mr => mr.Patient)
                                 .Include(mr => mr.Staff)
                                 .Include(mr => mr.Appointment)
                                 .Include(mr => mr.Service)
                                 .ToListAsync();

            if (!medicalRecords.Any())
            {
                return NotFound($"No medical records found for patient ID {patientId}.");
            }

            return medicalRecords.Select(mr => MapToMedicalRecordsDto(mr)).ToList();
        }

        // GET: api/MedicalRecords/doctor/{doctorId}
        /// <summary>
        /// Retrieves all medical records created or managed by a specific doctor (staff member).
        /// Requires Admin, HR, Doctor (their own records), or Nurse (their own created records).
        /// Admins/HR can optionally include deleted records for a doctor.
        /// </summary>
        /// <param name="doctorId">The ID of the staff member (doctor).</param>
        /// <param name="includeDeleted">Optional. If true and user is Admin/HR, includes soft-deleted records.</param>
        /// <returns>A list of MedicalRecordsDto created by the specified doctor.</returns>
        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Admin,HR,Doctor,Nurse")]
        public async Task<ActionResult<IEnumerable<MedicalRecordsDto>>> GetMedicalRecordsByDoctor(int doctorId, [FromQuery] bool includeDeleted = false)
        {
            // Custom authorization logic for Doctors/Nurses:
            if (User.IsInRole("Doctor") || User.IsInRole("Nurse"))
            {
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized("Could not identify current user.");
                }

                var staffIdForCurrentUser = await _context.StaffDetails
                                                            .Where(sd => sd.UserId == currentUserId)
                                                            .Select(sd => sd.StaffId)
                                                            .FirstOrDefaultAsync();

                if (staffIdForCurrentUser == 0 || staffIdForCurrentUser != doctorId)
                {
                    return Forbid("You can only view medical records associated with your own Staff ID.");
                }
            }

            if (_context.MedicalRecords == null)
            {
                return NotFound();
            }

            IQueryable<MedicalRecord> query = _context.MedicalRecords;

            // Admins/HR can retrieve deleted records for a specific doctor
            if (includeDeleted && (User.IsInRole("Admin") || User.IsInRole("HR")))
            {
                query = query.IgnoreQueryFilters();
            }

            var medicalRecords = await query
                                 .Where(mr => mr.StaffId == doctorId)
                                 .Include(mr => mr.Patient)
                                 .Include(mr => mr.Staff)
                                 .Include(mr => mr.Appointment)
                                 .Include(mr => mr.Service)
                                 .ToListAsync();

            if (!medicalRecords.Any())
            {
                return NotFound($"No medical records found for doctor (staff) ID {doctorId}.");
            }

            return medicalRecords.Select(mr => MapToMedicalRecordsDto(mr)).ToList();
        }


        // PUT: api/MedicalRecords/5
        /// <summary>
        /// Updates an existing medical record.
        /// Requires Admin, Doctor, or Nurse role.
        /// </summary>
        /// <param name="id">The ID of the medical record to update.</param>
        /// <param name="updateMedicalRecordsDto">The DTO object with updated data.</param>
        /// <returns>NoContent if successful, BadRequest if ID mismatch, NotFound if record not found, or throws exception for concurrency.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor,Nurse")]
        public async Task<IActionResult> PutMedicalRecord(int id, UpdateMedicalRecordsDto updateMedicalRecordsDto)
        {
            if (id != updateMedicalRecordsDto.RecordId)
            {
                return BadRequest("Mismatched Record ID in route and body.");
            }

            // Get the record, ignoring global query filters so we can update even if soft-deleted
            var medicalRecord = await _context.MedicalRecords.IgnoreQueryFilters().FirstOrDefaultAsync(mr => mr.RecordId == id);
            if (medicalRecord == null)
            {
                return NotFound();
            }

            // Optional: Add fine-grained authorization here for Doctors/Nurses modifying records
            if ((User.IsInRole("Doctor") || User.IsInRole("Nurse")))
            {
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized("Could not identify current user.");
                }
                var staffIdForCurrentUser = await _context.StaffDetails.Where(sd => sd.UserId == currentUserId).Select(sd => sd.StaffId).FirstOrDefaultAsync();
                // Ensure the staff creating/modifying the record is either an Admin/HR or is the specific staff associated with the record
                if (!(User.IsInRole("Admin") || User.IsInRole("HR") || medicalRecord.StaffId == staffIdForCurrentUser))
                {
                    return Forbid("You are not authorized to update this medical record.");
                }
            }


            // Manually map properties from DTO to the existing EF Core entity
            medicalRecord.PatientId = updateMedicalRecordsDto.PatientId;
            medicalRecord.AppointmentId = updateMedicalRecordsDto.AppointmentId;
            medicalRecord.StaffId = updateMedicalRecordsDto.StaffId;
            medicalRecord.ServiceId = updateMedicalRecordsDto.ServiceId;
            medicalRecord.Diagnosis = updateMedicalRecordsDto.Diagnosis;
            medicalRecord.Treatment = updateMedicalRecordsDto.Treatment;
            medicalRecord.Prescription = updateMedicalRecordsDto.Prescription;
            medicalRecord.UpdatedAt = DateTime.UtcNow; // Set update timestamp

            _context.Entry(medicalRecord).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MedicalRecordExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw; // Re-throw if it's a genuine concurrency issue
                }
            }

            return NoContent();
        }

        // POST: api/MedicalRecords
        /// <summary>
        /// Creates a new medical record.
        /// Requires Admin, Doctor, or Nurse role.
        /// </summary>
        /// <param name="createMedicalRecordsDto">The DTO object to create.</param>
        /// <returns>The created MedicalRecordsDto with its ID, or a problem if the DbSet is null.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor,Nurse")]
        public async Task<ActionResult<MedicalRecordsDto>> PostMedicalRecord(CreateMedicalRecordsDto createMedicalRecordsDto)
        {
            if (_context.MedicalRecords == null)
            {
                return Problem("Entity set 'ClinicManagementDbContext.MedicalRecords' is null.");
            }

            // Optional: Verify that the StaffId provided in the DTO matches the authenticated user's StaffId
            if ((User.IsInRole("Doctor") || User.IsInRole("Nurse")))
            {
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized("Could not identify current user.");
                }
                var staffIdForCurrentUser = await _context.StaffDetails.Where(sd => sd.UserId == currentUserId).Select(sd => sd.StaffId).FirstOrDefaultAsync();
                if (staffIdForCurrentUser == 0 || staffIdForCurrentUser != createMedicalRecordsDto.StaffId)
                {
                    return Forbid("You can only create medical records on behalf of your own Staff ID.");
                }
            }

            // Validate foreign keys exist and are not soft-deleted
            if (!await _context.Patients.AnyAsync(p => p.PatientId == createMedicalRecordsDto.PatientId && !p.IsDeleted))
            {
                return BadRequest($"Patient with ID {createMedicalRecordsDto.PatientId} does not exist or is deleted.");
            }
            if (!await _context.StaffDetails.AnyAsync(s => s.StaffId == createMedicalRecordsDto.StaffId && !s.IsDeleted))
            {
                return BadRequest($"Staff with ID {createMedicalRecordsDto.StaffId} does not exist or is deleted.");
            }
            if (createMedicalRecordsDto.AppointmentId.HasValue && !await _context.Appointments.AnyAsync(a => a.AppointmentId == createMedicalRecordsDto.AppointmentId.Value))
            {
                return BadRequest($"Appointment with ID {createMedicalRecordsDto.AppointmentId.Value} does not exist.");
            }
            if (createMedicalRecordsDto.ServiceId.HasValue && !await _context.Services.AnyAsync(s => s.ServiceId == createMedicalRecordsDto.ServiceId.Value))
            {
                return BadRequest($"Service with ID {createMedicalRecordsDto.ServiceId.Value} does not exist.");
            }


            var medicalRecord = new MedicalRecord
            {
                PatientId = createMedicalRecordsDto.PatientId,
                AppointmentId = createMedicalRecordsDto.AppointmentId,
                StaffId = createMedicalRecordsDto.StaffId,
                ServiceId = createMedicalRecordsDto.ServiceId,
                Diagnosis = createMedicalRecordsDto.Diagnosis,
                Treatment = createMedicalRecordsDto.Treatment,
                Prescription = createMedicalRecordsDto.Prescription,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false // Default to not deleted on creation
            };

            _context.MedicalRecords.Add(medicalRecord);
            await _context.SaveChangesAsync();

            // Load related entities before mapping to DTO for response
            await _context.Entry(medicalRecord)
                          .Reference(mr => mr.Patient).LoadAsync();
            await _context.Entry(medicalRecord)
                          .Reference(mr => mr.Staff).LoadAsync();
            await _context.Entry(medicalRecord)
                          .Reference(mr => mr.Appointment).LoadAsync();
            await _context.Entry(medicalRecord)
                          .Reference(mr => mr.Service).LoadAsync();


            return CreatedAtAction("GetMedicalRecord", new { id = medicalRecord.RecordId }, MapToMedicalRecordsDto(medicalRecord));
        }

        // DELETE: api/MedicalRecords/5 (Soft Delete)
        /// <summary>
        /// Soft deletes a medical record by marking it as IsDeleted = true.
        /// Requires Admin role.
        /// </summary>
        /// <param name="id">The ID of the medical record to soft delete.</param>
        /// <returns>NoContent if successful, or NotFound if the record does not exist.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can soft delete medical records
        public async Task<IActionResult> SoftDeleteMedicalRecord(int id)
        {
            if (_context.MedicalRecords == null)
            {
                return NotFound();
            }
            // Use IgnoreQueryFilters to find the record even if it's already soft-deleted
            var medicalRecord = await _context.MedicalRecords.IgnoreQueryFilters().FirstOrDefaultAsync(mr => mr.RecordId == id);
            if (medicalRecord == null)
            {
                return NotFound("Medical record not found.");
            }

            if (medicalRecord.IsDeleted)
            {
                return BadRequest("Medical record is already soft-deleted.");
            }

            medicalRecord.IsDeleted = true; // Mark as deleted
            medicalRecord.UpdatedAt = DateTime.UtcNow; // Update timestamp

            _context.Entry(medicalRecord).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Indicate successful soft delete
        }

        // POST: api/MedicalRecords/restore/5
        /// <summary>
        /// Restores a soft-deleted medical record by setting IsDeleted = false.
        /// Requires Admin role.
        /// </summary>
        /// <param name="id">The ID of the medical record to restore.</param>
        /// <returns>NoContent if successful, or NotFound if the record does not exist or is not deleted.</returns>
        [HttpPost("restore/{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can restore medical records
        public async Task<IActionResult> RestoreMedicalRecord(int id)
        {
            if (_context.MedicalRecords == null)
            {
                return NotFound();
            }
            // Use IgnoreQueryFilters to find the record even if it's soft-deleted
            var medicalRecord = await _context.MedicalRecords.IgnoreQueryFilters().FirstOrDefaultAsync(mr => mr.RecordId == id);
            if (medicalRecord == null)
            {
                return NotFound("Medical record not found.");
            }

            if (!medicalRecord.IsDeleted)
            {
                return BadRequest("Medical record is not soft-deleted.");
            }

            medicalRecord.IsDeleted = false; // Mark as not deleted
            medicalRecord.UpdatedAt = DateTime.UtcNow; // Update timestamp

            _context.Entry(medicalRecord).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Indicate successful restore
        }


        private bool MedicalRecordExists(int id)
        {
            // Use IgnoreQueryFilters here if you want MedicalRecordExists to consider soft-deleted records as 'existing'
            // Otherwise, it will only return true for active records due to the global filter.
            return (_context.MedicalRecords?.Any(e => e.RecordId == id)).GetValueOrDefault();
        }
    }
}
