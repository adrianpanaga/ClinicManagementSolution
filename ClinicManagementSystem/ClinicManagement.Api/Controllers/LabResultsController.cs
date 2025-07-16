using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.DTOs.LabResult;
using ClinicManagement.Api.DTOs.MedicalRecords;
using ClinicManagement.Api.DTOs.Patients;
using ClinicManagement.Api.DTOs.StaffDetails;
using ClinicManagement.Data.Context;
using ClinicManagement.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Lab results typically viewed by Doctors, Nurses, and Lab Techs. Admin/HR too.
    [Authorize(Roles = "Admin,HR,Doctor,Nurse,LabTech")]
    public class LabResultsController : ControllerBase
    {
        private readonly ClinicManagementDbContext _context;

        public LabResultsController(ClinicManagementDbContext context)
        {
            _context = context;
        }

        // GET: api/LabResults
        /// <summary>
        /// Retrieves all lab results. Admins/HR can optionally include deleted records.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Doctor,Nurse,LabTech")]
        public async Task<ActionResult<IEnumerable<LabResultDto>>> GetLabResults([FromQuery] bool includeDeleted = false)
        {
            var query = _context.LabResults
                .Include(lr => lr.Patient)
                .Include(lr => lr.MedicalRecord)
                .Include(lr => lr.Appointment)
                .Include(lr => lr.OrderedByStaff)
                .AsQueryable();

            if (includeDeleted && (User.IsInRole("Admin") || User.IsInRole("HR")))
            {
                query = query.IgnoreQueryFilters();
            }
            else
            {
                query = query.Where(lr => !lr.IsDeleted);
            }

            var results = await query.ToListAsync();

            return Ok(results.Select(lr => MapToLabResultDto(lr)));
        }

        // GET: api/LabResults/patient/{patientId}
        /// <summary>
        /// Retrieves all lab results for a specific patient.
        /// </summary>
        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Admin,HR,Doctor,Nurse,LabTech")]
        public async Task<ActionResult<IEnumerable<LabResultDto>>> GetLabResultsByPatient(int patientId, [FromQuery] bool includeDeleted = false)
        {
            var query = _context.LabResults
                .Where(lr => lr.PatientId == patientId);

            if (includeDeleted && (User.IsInRole("Admin") || User.IsInRole("HR")))
            {
                query = query.IgnoreQueryFilters();
            }
            else
            {
                query = query.Where(lr => !lr.IsDeleted);
            }

            var results = await query
                .Include(lr => lr.Patient)
                .Include(lr => lr.MedicalRecord)
                .Include(lr => lr.Appointment)
                .Include(lr => lr.OrderedByStaff)
                .OrderByDescending(lr => lr.ResultDate)
                .ThenByDescending(lr => lr.CreatedAt)
                .ToListAsync();

            if (!results.Any())
            {
                return NotFound($"No lab results found for patient ID: {patientId}");
            }

            return Ok(results.Select(lr => MapToLabResultDto(lr)));
        }

        // GET: api/LabResults/5
        /// <summary>
        /// Retrieves a specific lab result by ID.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,HR,Doctor,Nurse,LabTech")]
        public async Task<ActionResult<LabResultDto>> GetLabResult(int id)
        {
            var query = _context.LabResults
                .Include(lr => lr.Patient)
                .Include(lr => lr.MedicalRecord)
                .Include(lr => lr.Appointment)
                .Include(lr => lr.OrderedByStaff)
                .AsQueryable();

            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                query = query.IgnoreQueryFilters();
            }
            else
            {
                query = query.Where(lr => !lr.IsDeleted);
            }

            var labResult = await query.FirstOrDefaultAsync(lr => lr.LabResultId == id);

            if (labResult == null)
            {
                return NotFound();
            }

            return MapToLabResultDto(labResult);
        }

        // POST: api/LabResults
        /// <summary>
        /// Creates a new lab result.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "LabTech,Doctor,Nurse,Admin")] // Typically LabTechs, Doctors, Nurses create these
        public async Task<ActionResult<LabResultDto>> PostLabResult([FromBody] CreateLabResultDto createDto)
        {
            // Get current staff ID from JWT (optional, if you want to auto-assign OrderedByStaffId)
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            int? currentStaffId = null;
            if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int userId))
            {
                currentStaffId = await _context.StaffDetails.Where(sd => sd.UserId == userId).Select(sd => sd.StaffId).FirstOrDefaultAsync();
            }

            var labResult = new LabResult
            {
                PatientId = createDto.PatientId,
                MedicalRecordId = createDto.MedicalRecordId,
                AppointmentId = createDto.AppointmentId,
                TestName = createDto.TestName,
                ResultValue = createDto.ResultValue,
                Unit = createDto.Unit,
                ReferenceRange = createDto.ReferenceRange,
                Interpretation = createDto.Interpretation,
                ResultDate = createDto.ResultDate.ToDateTime(TimeOnly.MinValue), // Convert DateOnly to DateTime for model
                OrderedByStaffId = createDto.OrderedByStaffId ?? currentStaffId, // Use provided StaffId or current logged-in staff
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.LabResults.Add(labResult);
            await _context.SaveChangesAsync();

            // Load related entities for the DTO response
            await _context.Entry(labResult).Reference(lr => lr.Patient).LoadAsync();
            if (labResult.MedicalRecordId.HasValue) await _context.Entry(labResult).Reference(lr => lr.MedicalRecord).LoadAsync();
            if (labResult.AppointmentId.HasValue) await _context.Entry(labResult).Reference(lr => lr.Appointment).LoadAsync();
            if (labResult.OrderedByStaffId.HasValue) await _context.Entry(labResult).Reference(lr => lr.OrderedByStaff).LoadAsync();


            return CreatedAtAction(nameof(GetLabResult), new { id = labResult.LabResultId }, MapToLabResultDto(labResult));
        }

        // PUT: api/LabResults/5
        /// <summary>
        /// Updates an existing lab result.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "LabTech,Doctor,Nurse,Admin")] // Roles allowed to modify
        public async Task<IActionResult> PutLabResult(int id, [FromBody] UpdateLabResultDto updateDto)
        {
            if (id != updateDto.LabResultId)
            {
                return BadRequest("Lab Result ID mismatch.");
            }

            var labResult = await _context.LabResults
                .IgnoreQueryFilters() // Ignore filter for update to find the record even if soft-deleted
                .FirstOrDefaultAsync(lr => lr.LabResultId == id);

            if (labResult == null)
            {
                return NotFound();
            }

            // Update properties
            // Only update if provided in DTO (nullables)
            labResult.MedicalRecordId = updateDto.MedicalRecordId;
            labResult.AppointmentId = updateDto.AppointmentId;
            labResult.TestName = updateDto.TestName ?? labResult.TestName; // Null-coalescing for string
            labResult.ResultValue = updateDto.ResultValue ?? labResult.ResultValue;
            labResult.Unit = updateDto.Unit ?? labResult.Unit;
            labResult.ReferenceRange = updateDto.ReferenceRange ?? labResult.ReferenceRange;
            labResult.Interpretation = updateDto.Interpretation ?? labResult.Interpretation;
            if (updateDto.ResultDate.HasValue)
            {
                labResult.ResultDate = updateDto.ResultDate.Value.ToDateTime(TimeOnly.MinValue); // Convert DateOnly to DateTime
            }
            labResult.OrderedByStaffId = updateDto.OrderedByStaffId; // Can explicitly set or null
            labResult.UpdatedAt = DateTime.UtcNow;
            labResult.IsDeleted = updateDto.IsDeleted; // Allow setting soft delete status

            _context.Entry(labResult).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LabResultExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // 204 No Content
        }

        // DELETE: api/LabResults/5 (Soft Delete)
        /// <summary>
        /// Soft deletes a lab result.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,LabTech")] // Admins or LabTechs can soft delete
        public async Task<IActionResult> DeleteLabResult(int id)
        {
            var labResult = await _context.LabResults
                .IgnoreQueryFilters() // Ignore filter to find deleted records
                .FirstOrDefaultAsync(lr => lr.LabResultId == id);

            if (labResult == null)
            {
                return NotFound();
            }

            labResult.IsDeleted = true; // Perform soft delete
            labResult.UpdatedAt = DateTime.UtcNow;
            _context.Entry(labResult).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/LabResults/restore/5
        /// <summary>
        /// Restores a soft-deleted lab result.
        /// </summary>
        [HttpPost("restore/{id}")]
        [Authorize(Roles = "Admin,LabTech")] // Admins or LabTechs can restore
        public async Task<IActionResult> RestoreLabResult(int id)
        {
            var labResult = await _context.LabResults
                .IgnoreQueryFilters() // Ignore filter to find deleted records
                .FirstOrDefaultAsync(lr => lr.LabResultId == id);

            if (labResult == null)
            {
                return NotFound();
            }

            labResult.IsDeleted = false; // Restore
            labResult.UpdatedAt = DateTime.UtcNow;
            _context.Entry(labResult).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LabResultExists(int id)
        {
            return (_context.LabResults?.Any(e => e.LabResultId == id)).GetValueOrDefault();
        }

        // Helper method to map LabResult entity to DTO
        private LabResultDto MapToLabResultDto(LabResult lr)
        {
            return new LabResultDto
            {
                LabResultId = lr.LabResultId,
                PatientId = lr.PatientId,
                MedicalRecordId = lr.MedicalRecordId,
                AppointmentId = lr.AppointmentId,
                TestName = lr.TestName,
                ResultValue = lr.ResultValue,
                Unit = lr.Unit,
                ReferenceRange = lr.ReferenceRange,
                Interpretation = lr.Interpretation,
                ResultDate = DateOnly.FromDateTime(lr.ResultDate), // Convert DateTime to DateOnly
                OrderedByStaffId = lr.OrderedByStaffId,
                CreatedAt = lr.CreatedAt,
                UpdatedAt = lr.UpdatedAt,
                IsDeleted = lr.IsDeleted,
                Patient = lr.Patient != null ? new PatientDetailsDto
                {
                    PatientId = lr.Patient.PatientId,
                    FirstName = lr.Patient.FirstName,
                    MiddleName = lr.Patient.MiddleName,
                    LastName = lr.Patient.LastName,
                    ContactNumber = lr.Patient.ContactNumber,
                    Email = lr.Patient.Email,
                    DateOfBirth = lr.Patient.DateOfBirth.HasValue ? DateOnly.FromDateTime(lr.Patient.DateOfBirth.Value) : (DateOnly?)null
                } : null,
                MedicalRecord = lr.MedicalRecord != null ? new MedicalRecordsDto
                {
                    RecordId = lr.MedicalRecord.RecordId,
                    Diagnosis = lr.MedicalRecord.Diagnosis,
                    Treatment = lr.MedicalRecord.Treatment,
                    Prescription = lr.MedicalRecord.Prescription,
                    CreatedAt = lr.MedicalRecord.CreatedAt
                } : null,
                Appointment = lr.Appointment != null ? new AppointmentDto
                {
                    AppointmentId = lr.Appointment.AppointmentId,
                    AppointmentDateTime = lr.Appointment.AppointmentDateTime,
                    Notes = lr.Appointment.Notes,
                    Status = lr.Appointment.Status
                } : null,
                OrderedByStaff = lr.OrderedByStaff != null ? new StaffDetailDto
                {
                    StaffId = lr.OrderedByStaff.StaffId,
                    FirstName = lr.OrderedByStaff.FirstName,
                    MiddleName = lr.OrderedByStaff.MiddleName,
                    LastName = lr.OrderedByStaff.LastName,
                    JobTitle = lr.OrderedByStaff.JobTitle
                } : null
            };
        }
    }
}
