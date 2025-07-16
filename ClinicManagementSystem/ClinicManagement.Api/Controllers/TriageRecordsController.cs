using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.DTOs.Patients;
using ClinicManagement.Api.DTOs.TriageRecords;
using ClinicManagement.Data.Context;
using ClinicManagement.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Nurse,Doctor,Receptionist")]
    public class TriageRecordsController : ControllerBase
    {
        private readonly ClinicManagementDbContext _context;

        public TriageRecordsController(ClinicManagementDbContext context)
        {
            _context = context;
        }

        // GET: api/TriageRecords
        /// <summary>
        /// Retrieves all triage records. Admins/HR can optionally include deleted records.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Nurse,Doctor,Receptionist")] // Roles allowed to view all triage records
        public async Task<ActionResult<IEnumerable<TriageRecordDto>>> GetTriageRecords([FromQuery] bool includeDeleted = false)
        {
            var query = _context.TriageRecords
                .Include(tr => tr.Patient)
                .Include(tr => tr.Appointment)
                .AsQueryable();

            // Handle soft-delete filter bypass for Admin/HR
            if (includeDeleted && (User.IsInRole("Admin") || User.IsInRole("HR")))
            {
                query = query.IgnoreQueryFilters();
            }
            // If includeDeleted is false or user not Admin/HR, global filter should apply.

            var records = await query.ToListAsync();

            // Map to DTOs
            return Ok(records.Select(tr => MapToTriageRecordDto(tr)));
        }

        // GET: api/TriageRecords/patient/{patientId}
        /// <summary>
        /// Retrieves all triage records for a specific patient.
        /// </summary>
        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Admin,Nurse,Doctor,Receptionist")] // Roles allowed to view a patient's triage history
        public async Task<ActionResult<IEnumerable<TriageRecordDto>>> GetTriageRecordsByPatient(int patientId, [FromQuery] bool includeDeleted = false)
        {
            var query = _context.TriageRecords
                .Where(tr => tr.PatientId == patientId); // Filter by PatientId

            // Handle soft-delete filter bypass for Admin/HR
            if (includeDeleted && (User.IsInRole("Admin") || User.IsInRole("HR")))
            {
                query = query.IgnoreQueryFilters();
            }

            var records = await query
                .Include(tr => tr.Patient)
                .Include(tr => tr.Appointment)
                .OrderByDescending(tr => tr.CreatedAt) // Order by most recent first
                .ToListAsync();

            if (!records.Any())
            {
                return NotFound($"No triage records found for patient ID: {patientId}");
            }

            return Ok(records.Select(tr => MapToTriageRecordDto(tr)));
        }

        // GET: api/TriageRecords/5
        /// <summary>
        /// Retrieves a specific triage record by ID.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Nurse,Doctor,Receptionist")]
        public async Task<ActionResult<TriageRecordDto>> GetTriageRecord(int id)
        {
            var query = _context.TriageRecords
                .Include(tr => tr.Patient)
                .Include(tr => tr.Appointment)
                .AsQueryable();

            // Handle soft-delete filter bypass for Admin/HR
            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                query = query.IgnoreQueryFilters();
            }
            else // Ensure non-admin/HR only see non-deleted records
            {
                query = query.Where(tr => !tr.IsDeleted);
            }

            var triageRecord = await query.FirstOrDefaultAsync(tr => tr.TriageRecordId == id); // Use TriageRecordId

            if (triageRecord == null)
            {
                return NotFound();
            }

            return MapToTriageRecordDto(triageRecord);
        }

        // POST: api/TriageRecords
        /// <summary>
        /// Creates a new triage record.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Nurse,Receptionist,Admin")] // Typically Nurses/Receptionists create these
        public async Task<ActionResult<TriageRecordDto>> PostTriageRecord([FromBody] CreateTriageRecordDto createDto)
        {
            var triageRecord = new TriageRecord
            {
                PatientId = createDto.PatientId,
                AppointmentId = createDto.AppointmentId,
                ChiefComplaint = createDto.ChiefComplaint,
                Temperature = createDto.Temperature,
                BloodPressureSystolic = createDto.BloodPressureSystolic,
                BloodPressureDiastolic = createDto.BloodPressureDiastolic,
                PulseRate = createDto.PulseRate,
                RespiratoryRate = createDto.RespiratoryRate,
                Weight = createDto.Weight,
                Height = createDto.Height,
                Notes = createDto.Notes,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.TriageRecords.Add(triageRecord);
            await _context.SaveChangesAsync();

            // Load related entities for the DTO response
            await _context.Entry(triageRecord).Reference(tr => tr.Patient).LoadAsync();
            if (triageRecord.AppointmentId.HasValue)
            {
                await _context.Entry(triageRecord).Reference(tr => tr.Appointment).LoadAsync();
            }

            return CreatedAtAction(nameof(GetTriageRecord), new { id = triageRecord.TriageRecordId }, MapToTriageRecordDto(triageRecord));
        }

        // PUT: api/TriageRecords/5
        /// <summary>
        /// Updates an existing triage record.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Nurse,Doctor,Admin")] // Roles allowed to modify
        public async Task<IActionResult> PutTriageRecord(int id, [FromBody] UpdateTriageRecordDto updateDto)
        {
            if (id != updateDto.TriageRecordId) // Use TriageRecordId for consistency
            {
                return BadRequest("Triage Record ID mismatch.");
            }

            var triageRecord = await _context.TriageRecords
                .IgnoreQueryFilters() // Ignore filter for update to find the record even if soft-deleted
                .FirstOrDefaultAsync(tr => tr.TriageRecordId == id); // Use TriageRecordId

            if (triageRecord == null)
            {
                return NotFound();
            }

            // Update properties
            triageRecord.AppointmentId = updateDto.AppointmentId;
            triageRecord.ChiefComplaint = updateDto.ChiefComplaint;
            triageRecord.Temperature = updateDto.Temperature;
            triageRecord.BloodPressureSystolic = updateDto.BloodPressureSystolic;
            triageRecord.BloodPressureDiastolic = updateDto.BloodPressureDiastolic;
            triageRecord.PulseRate = updateDto.PulseRate;
            triageRecord.RespiratoryRate = updateDto.RespiratoryRate;
            triageRecord.Weight = updateDto.Weight;
            triageRecord.Height = updateDto.Height;
            triageRecord.Notes = updateDto.Notes;
            triageRecord.UpdatedAt = DateTime.UtcNow;
            triageRecord.IsDeleted = updateDto.IsDeleted; // Allow setting soft delete status

            _context.Entry(triageRecord).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TriageRecordExists(id))
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

        // DELETE: api/TriageRecords/5 (Soft Delete)
        /// <summary>
        /// Soft deletes a triage record.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Nurse")] // Admins or Nurses can soft delete
        public async Task<IActionResult> DeleteTriageRecord(int id)
        {
            var triageRecord = await _context.TriageRecords
                .IgnoreQueryFilters() // Ignore filter to find deleted records
                .FirstOrDefaultAsync(tr => tr.TriageRecordId == id); // Use TriageRecordId

            if (triageRecord == null)
            {
                return NotFound();
            }

            triageRecord.IsDeleted = true; // Perform soft delete
            triageRecord.UpdatedAt = DateTime.UtcNow;
            _context.Entry(triageRecord).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/TriageRecords/restore/5
        /// <summary>
        /// Restores a soft-deleted triage record.
        /// </summary>
        [HttpPost("restore/{id}")]
        [Authorize(Roles = "Admin,Nurse")] // Admins or Nurses can restore
        public async Task<IActionResult> RestoreTriageRecord(int id)
        {
            var triageRecord = await _context.TriageRecords
                .IgnoreQueryFilters() // Ignore filter to find deleted records
                .FirstOrDefaultAsync(tr => tr.TriageRecordId == id); // Use TriageRecordId

            if (triageRecord == null)
            {
                return NotFound();
            }

            triageRecord.IsDeleted = false; // Restore
            triageRecord.UpdatedAt = DateTime.UtcNow;
            _context.Entry(triageRecord).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TriageRecordExists(int id)
        {
            return (_context.TriageRecords?.Any(e => e.TriageRecordId == id)).GetValueOrDefault(); // Use TriageRecordId
        }

        // Helper method to map TriageRecord entity to DTO
        private TriageRecordDto MapToTriageRecordDto(TriageRecord tr) => new TriageRecordDto
        {
            TriageRecordId = tr.TriageRecordId, // Use TriageRecordId
            PatientId = tr.PatientId,
            AppointmentId = tr.AppointmentId,
            ChiefComplaint = tr.ChiefComplaint,
            Temperature = tr.Temperature,
            BloodPressureSystolic = tr.BloodPressureSystolic,
            BloodPressureDiastolic = tr.BloodPressureDiastolic,
            PulseRate = tr.PulseRate,
            RespiratoryRate = tr.RespiratoryRate,
            Weight = tr.Weight,
            Height = tr.Height,
            Notes = tr.Notes,
            CreatedAt = tr.CreatedAt,
            UpdatedAt = tr.UpdatedAt,
            IsDeleted = tr.IsDeleted,
            Patient = tr.Patient != null ? new PatientDetailsDto
            {
                PatientId = tr.Patient.PatientId,
                FirstName = tr.Patient.FirstName,
                LastName = tr.Patient.LastName,
                ContactNumber = tr.Patient.ContactNumber,
                Email = tr.Patient.Email,
                DateOfBirth = tr.Patient.DateOfBirth.HasValue ? DateOnly.FromDateTime(tr.Patient.DateOfBirth.Value) : (DateOnly?)null,
            } : null,
            Appointment = tr.Appointment != null ? new AppointmentDto
            {
                AppointmentId = tr.Appointment.AppointmentId,
                AppointmentDateTime = tr.Appointment.AppointmentDateTime,
                Notes = tr.Appointment.Notes,
                Status = tr.Appointment.Status
            } : null
        };
    }
}
