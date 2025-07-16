using ClinicManagement.Api.DTOs.PatientDocuments;
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
    // Documents typically managed by Admins, Receptionists, and potentially Doctors/Nurses/HR
    [Authorize(Roles = "Admin,HR,Receptionist,Doctor,Nurse")]
    public class PatientDocumentsController : ControllerBase
    {
        private readonly ClinicManagementDbContext _context;

        public PatientDocumentsController(ClinicManagementDbContext context)
        {
            _context = context;
        }

        // GET: api/PatientDocuments
        /// <summary>
        /// Retrieves all patient documents. Admins/HR can optionally include deleted records.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Receptionist,Doctor,Nurse")]
        public async Task<ActionResult<IEnumerable<PatientDocumentDto>>> GetPatientDocuments([FromQuery] bool includeDeleted = false)
        {
            var query = _context.PatientDocuments
                .Include(pd => pd.Patient)
                .Include(pd => pd.UploadedByStaff)
                .AsQueryable();

            if (includeDeleted && (User.IsInRole("Admin") || User.IsInRole("HR")))
            {
                query = query.IgnoreQueryFilters();
            }
            else
            {
                query = query.Where(pd => !pd.IsDeleted);
            }

            var results = await query.ToListAsync();

            return Ok(results.Select(pd => MapToPatientDocumentDto(pd)));
        }

        // GET: api/PatientDocuments/patient/{patientId}
        /// <summary>
        /// Retrieves all documents for a specific patient.
        /// </summary>
        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Admin,HR,Receptionist,Doctor,Nurse")]
        public async Task<ActionResult<IEnumerable<PatientDocumentDto>>> GetPatientDocumentsByPatient(int patientId, [FromQuery] bool includeDeleted = false)
        {
            var query = _context.PatientDocuments
                .Where(pd => pd.PatientId == patientId);

            if (includeDeleted && (User.IsInRole("Admin") || User.IsInRole("HR")))
            {
                query = query.IgnoreQueryFilters();
            }
            else
            {
                query = query.Where(pd => !pd.IsDeleted);
            }

            var results = await query
                .Include(pd => pd.Patient)
                .Include(pd => pd.UploadedByStaff)
                .OrderByDescending(pd => pd.UploadDate)
                .ThenByDescending(pd => pd.CreatedAt)
                .ToListAsync();

            if (!results.Any())
            {
                return NotFound($"No documents found for patient ID: {patientId}");
            }

            return Ok(results.Select(pd => MapToPatientDocumentDto(pd)));
        }

        // GET: api/PatientDocuments/5
        /// <summary>
        /// Retrieves a specific patient document by ID.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,HR,Receptionist,Doctor,Nurse")]
        public async Task<ActionResult<PatientDocumentDto>> GetPatientDocument(int id)
        {
            var query = _context.PatientDocuments
                .Include(pd => pd.Patient)
                .Include(pd => pd.UploadedByStaff)
                .AsQueryable();

            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                query = query.IgnoreQueryFilters();
            }
            else
            {
                query = query.Where(pd => !pd.IsDeleted);
            }

            var document = await query.FirstOrDefaultAsync(pd => pd.DocumentId == id);

            if (document == null)
            {
                return NotFound();
            }

            return MapToPatientDocumentDto(document);
        }

        // POST: api/PatientDocuments
        /// <summary>
        /// Creates a new patient document record.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Receptionist,HR,Admin")] // Typically Receptionists, HR, Admins upload documents
        public async Task<ActionResult<PatientDocumentDto>> PostPatientDocument([FromBody] CreatePatientDocumentDto createDto)
        {
            // Get current staff ID from JWT (optional, if you want to auto-assign UploadedByStaffId)
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            int? currentStaffId = null;
            if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int userId))
            {
                currentStaffId = await _context.StaffDetails.Where(sd => sd.UserId == userId).Select(sd => sd.StaffId).FirstOrDefaultAsync();
            }

            var patientDocument = new PatientDocument
            {
                PatientId = createDto.PatientId,
                DocumentName = createDto.DocumentName,
                DocumentType = createDto.DocumentType,
                FilePathOrUrl = createDto.FilePathOrUrl,
                Notes = createDto.Notes,
                UploadedByStaffId = createDto.UploadedByStaffId ?? currentStaffId, // Use provided StaffId or current logged-in staff
                UploadDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.PatientDocuments.Add(patientDocument);
            await _context.SaveChangesAsync();

            // Load related entities for the DTO response
            await _context.Entry(patientDocument).Reference(pd => pd.Patient).LoadAsync();
            if (patientDocument.UploadedByStaffId.HasValue) await _context.Entry(patientDocument).Reference(pd => pd.UploadedByStaff).LoadAsync();


            return CreatedAtAction(nameof(GetPatientDocument), new { id = patientDocument.DocumentId }, MapToPatientDocumentDto(patientDocument));
        }

        // PUT: api/PatientDocuments/5
        /// <summary>
        /// Updates an existing patient document record.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Receptionist,HR,Admin")] // Roles allowed to modify
        public async Task<IActionResult> PutPatientDocument(int id, [FromBody] UpdatePatientDocumentDto updateDto)
        {
            if (id != updateDto.DocumentId)
            {
                return BadRequest("Document ID mismatch.");
            }

            var patientDocument = await _context.PatientDocuments
                .IgnoreQueryFilters() // Ignore filter for update to find the record even if soft-deleted
                .FirstOrDefaultAsync(pd => pd.DocumentId == id);

            if (patientDocument == null)
            {
                return NotFound();
            }

            // Update properties (only if provided in DTO for strings)
            patientDocument.DocumentName = updateDto.DocumentName ?? patientDocument.DocumentName;
            patientDocument.DocumentType = updateDto.DocumentType ?? patientDocument.DocumentType;
            patientDocument.FilePathOrUrl = updateDto.FilePathOrUrl ?? patientDocument.FilePathOrUrl;
            patientDocument.Notes = updateDto.Notes ?? patientDocument.Notes;
            patientDocument.UploadedByStaffId = updateDto.UploadedByStaffId; // Can explicitly set or null
            patientDocument.UpdatedAt = DateTime.UtcNow;
            patientDocument.IsDeleted = updateDto.IsDeleted; // Allow setting soft delete status

            _context.Entry(patientDocument).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientDocumentExists(id))
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

        // DELETE: api/PatientDocuments/5 (Soft Delete)
        /// <summary>
        /// Soft deletes a patient document record.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,HR")] // Admins or HR can soft delete documents
        public async Task<IActionResult> DeletePatientDocument(int id)
        {
            var patientDocument = await _context.PatientDocuments
                .IgnoreQueryFilters() // Ignore filter to find deleted records
                .FirstOrDefaultAsync(pd => pd.DocumentId == id);

            if (patientDocument == null)
            {
                return NotFound();
            }

            patientDocument.IsDeleted = true; // Perform soft delete
            patientDocument.UpdatedAt = DateTime.UtcNow;
            _context.Entry(patientDocument).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/PatientDocuments/restore/5
        /// <summary>
        /// Restores a soft-deleted patient document record.
        /// </summary>
        [HttpPost("restore/{id}")]
        [Authorize(Roles = "Admin,HR")] // Admins or HR can restore
        public async Task<IActionResult> RestorePatientDocument(int id)
        {
            var patientDocument = await _context.PatientDocuments
                .IgnoreQueryFilters() // Ignore filter to find deleted records
                .FirstOrDefaultAsync(pd => pd.DocumentId == id);

            if (patientDocument == null)
            {
                return NotFound();
            }

            patientDocument.IsDeleted = false; // Restore
            patientDocument.UpdatedAt = DateTime.UtcNow;
            _context.Entry(patientDocument).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PatientDocumentExists(int id)
        {
            return (_context.PatientDocuments?.Any(e => e.DocumentId == id)).GetValueOrDefault();
        }

        // Helper method to map PatientDocument entity to DTO
        private PatientDocumentDto MapToPatientDocumentDto(PatientDocument pd)
        {
            return new PatientDocumentDto
            {
                DocumentId = pd.DocumentId,
                PatientId = pd.PatientId,
                DocumentName = pd.DocumentName,
                DocumentType = pd.DocumentType,
                FilePathOrUrl = pd.FilePathOrUrl,
                Notes = pd.Notes,
                UploadedByStaffId = pd.UploadedByStaffId,
                UploadDate = pd.UploadDate,
                CreatedAt = pd.CreatedAt,
                UpdatedAt = pd.UpdatedAt,
                IsDeleted = pd.IsDeleted,
                Patient = pd.Patient != null ? new PatientDetailsDto
                {
                    PatientId = pd.Patient.PatientId,
                    FirstName = pd.Patient.FirstName,
                    MiddleName = pd.Patient.MiddleName,
                    LastName = pd.Patient.LastName,
                    ContactNumber = pd.Patient.ContactNumber,
                    Email = pd.Patient.Email,
                    DateOfBirth = pd.Patient.DateOfBirth.HasValue ? DateOnly.FromDateTime(pd.Patient.DateOfBirth.Value) : (DateOnly?)null
                } : null,
                UploadedByStaff = pd.UploadedByStaff != null ? new StaffDetailDto
                {
                    StaffId = pd.UploadedByStaff.StaffId,
                    FirstName = pd.UploadedByStaff.FirstName,
                    LastName = pd.UploadedByStaff.LastName,
                    JobTitle = pd.UploadedByStaff.JobTitle
                } : null
            };
        }
    }
}
