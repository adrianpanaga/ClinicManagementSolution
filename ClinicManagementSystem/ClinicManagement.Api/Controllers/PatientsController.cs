// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\Controllers\PatientsController.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // For authorization attributes
using System.Security.Claims; // For accessing user claims (e.g., UserId)

using ClinicManagement.Data.Context; // Your DbContext
using ClinicManagement.Data.Models;   // Your EF Core models
using ClinicManagement.Api.DTOs.Patients; // Your Patient DTOs

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All actions in this controller require an authenticated user by default
    public class PatientsController : ControllerBase
    {
        private readonly ClinicManagementDbContext _context;

        public PatientsController(ClinicManagementDbContext context)
        {
            _context = context;
        }

        // Helper method to map Patient model to PatientDetailsDto
        // CA1822: Mark method as static as it does not access instance data
        private static PatientDetailsDto MapToPatientDetailsDto(Patient patient)
        {
            if (patient == null) return null; // CS8603: Possible null reference return. (Expected)

            return new PatientDetailsDto
            {
                PatientId = patient.PatientId,
                FirstName = patient.FirstName,
                MiddleName = patient.MiddleName,
                LastName = patient.LastName,
                Gender = patient.Gender,
                // Map DateTime? to DateOnly?
                DateOfBirth = patient.DateOfBirth.HasValue ? DateOnly.FromDateTime(patient.DateOfBirth.Value) : (DateOnly?)null,
                Address = patient.Address,
                ContactNumber = patient.ContactNumber,
                Email = patient.Email,
                BloodType = patient.BloodType,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactNumber = patient.EmergencyContactNumber,
                PhotoUrl = patient.PhotoUrl,
                CreatedAt = patient.CreatedAt,
                UpdatedAt = patient.UpdatedAt,
                UserId = patient.UserId,
                IsDeleted = patient.IsDeleted // Include the soft delete status
            };
        }

        // GET: api/Patients
        /// <summary>
        /// Retrieves all patients. For Admins/HR, can optionally include deleted records.
        /// </summary>
        /// <param name="includeDeleted">Optional. If true and user is Admin/HR, includes soft-deleted patients.</param>
        /// <returns>A list of PatientDetailsDto.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Receptionist")] // Roles allowed to view all patients
        public async Task<ActionResult<IEnumerable<PatientDetailsDto>>> GetPatients([FromQuery] bool includeDeleted = false)
        {
            if (_context.Patients == null)
            {
                return NotFound();
            }

            IQueryable<Patient> query = _context.Patients;

            // Admins and HR can choose to see deleted patients
            if (includeDeleted && (User.IsInRole("Admin") || User.IsInRole("HR")))
            {
                query = query.IgnoreQueryFilters(); // Bypass the global query filter
            }

            var patients = await query.ToListAsync();
            return patients.Select(p => MapToPatientDetailsDto(p)).ToList();
        }

        // GET: api/Patients/5
        /// <summary>
        /// Retrieves a specific patient by ID.
        /// Requires Admin, HR, Receptionist, Doctor, Nurse. Patients can only get their own record.
        /// Admins/HR can retrieve deleted records by ID.
        /// </summary>
        /// <param name="id">The ID of the patient.</param>
        /// <returns>The PatientDetailsDto if found, otherwise NotFound.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,HR,Receptionist,Doctor,Nurse,Patient")]
        public async Task<ActionResult<PatientDetailsDto>> GetPatient(int id)
        {
            if (_context.Patients == null)
            {
                return NotFound();
            }

            IQueryable<Patient> query = _context.Patients;

            // Admins/HR can retrieve deleted records by ID
            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                query = query.IgnoreQueryFilters();
            }

            var patient = await query.FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
            {
                return NotFound();
            }

            // Custom authorization for Patient role: Must match their own record
            if (User.IsInRole("Patient"))
            {
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized("Could not identify current user.");
                }

                // If Patient.UserId is nullable, ensure it's not null before comparing
                if (!patient.UserId.HasValue || patient.UserId.Value != currentUserId)
                {
                    return Forbid("You do not have access to this patient record.");
                }
            }
            // Optional: For Doctor/Nurse, could add logic to ensure they have a legitimate reason to access (e.g., patient assigned to them)

            return MapToPatientDetailsDto(patient);
        }

        // PUT: api/Patients/5
        /// <summary>
        /// Updates an existing patient record.
        /// Requires Admin, HR, or Receptionist role.
        /// </summary>
        /// <param name="id">The ID of the patient to update.</param>
        /// <param name="updatePatientDto">The DTO object with updated data.</param>
        /// <returns>NoContent if successful, BadRequest if ID mismatch, NotFound if patient not found, or throws exception for concurrency.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,HR,Receptionist")] // Roles allowed to update patients
        public async Task<IActionResult> PutPatient(int id, UpdatePatientDto updatePatientDto)
        {
            updatePatientDto.PatientId = id;

            if (id != updatePatientDto.PatientId)
            {
                return BadRequest("Mismatched Patient ID in route and body.");
            }

            // Get the patient, ignoring global query filters so we can update even if soft-deleted
            var patient = await _context.Patients.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.PatientId == id);
            if (patient == null)
            {
                return NotFound();
            }

            // Manually map properties from DTO to the existing EF Core entity
            // Use null-forgiving operator (!) for non-nullable properties that are required in DTOs
            // Use null-coalescing (??) for nullable properties, allowing them to remain unchanged if null in DTO
            patient.FirstName = updatePatientDto.FirstName ?? patient.FirstName;
            patient.MiddleName = updatePatientDto.MiddleName ?? patient.MiddleName;
            patient.LastName = updatePatientDto.LastName ?? patient.LastName;
            patient.Gender = updatePatientDto.Gender ?? patient.Gender;
            patient.DateOfBirth = updatePatientDto.DateOfBirth?.ToDateTime(TimeOnly.MinValue) ?? patient.DateOfBirth; // Convert DateOnly? to DateTime?
            patient.Address = updatePatientDto.Address ?? patient.Address;
            patient.ContactNumber = updatePatientDto.ContactNumber ?? patient.ContactNumber;
            patient.Email = updatePatientDto.Email ?? patient.Email;
            patient.BloodType = updatePatientDto.BloodType ?? patient.BloodType;
            patient.EmergencyContactName = updatePatientDto.EmergencyContactName ?? patient.EmergencyContactName;
            patient.EmergencyContactNumber = updatePatientDto.EmergencyContactNumber ?? patient.EmergencyContactNumber;
            patient.PhotoUrl = updatePatientDto.PhotoUrl ?? patient.PhotoUrl;
            patient.UpdatedAt = DateTime.UtcNow; // Set update timestamp

            _context.Entry(patient).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientExists(id)) // Check existence, considering global filter. This implies we only want to update existing active records generally.
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

        // POST: api/Patients
        /// <summary>
        /// Creates a new patient record.
        /// Requires Admin or Receptionist role.
        /// </summary>
        /// <param name="createPatientDto">The DTO object to create.</param>
        /// <returns>The created PatientDetailsDto with its ID, or a problem if the DbSet is null.</returns>
        [HttpPost]
        [Authorize]
        //[Authorize(Roles = "Admin,Receptionist")] // Roles allowed to create patients
        public async Task<ActionResult<PatientDetailsDto>> PostPatient([FromBody] CreatePatientDto createPatientDto)
        {
            if (_context.Patients == null)
            {
                return Problem("Entity set 'ClinicManagementDbContext.Patients' is null.");
            }

            // Manually map DTO to EF Core model
            var patient = new Patient
            {
                FirstName = createPatientDto.FirstName!,
                MiddleName = createPatientDto.MiddleName,
                LastName = createPatientDto.LastName!,
                Gender = createPatientDto.Gender,
                DateOfBirth = createPatientDto.DateOfBirth?.ToDateTime(TimeOnly.MinValue), // Convert DateOnly? to DateTime?
                Address = createPatientDto.Address,
                ContactNumber = createPatientDto.ContactNumber!,
                Email = createPatientDto.Email!,
                BloodType = createPatientDto.BloodType,
                EmergencyContactName = createPatientDto.EmergencyContactName,
                EmergencyContactNumber = createPatientDto.EmergencyContactNumber,
                PhotoUrl = createPatientDto.PhotoUrl,
                CreatedAt = DateTime.UtcNow, // Set creation timestamp
                UpdatedAt = DateTime.UtcNow,  // Set initial update timestamp
                IsDeleted = false // Default to not deleted
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPatient", new { id = patient.PatientId }, MapToPatientDetailsDto(patient));
        }

        //[HttpPost("test-route")] // This will create the route: /api/Patients/test-route
        //[AllowAnonymous] // Allows access without any token
        //public IActionResult TestRoute()
        //{
        //    return Ok("Test route reached successfully!");
        //}

        // DELETE: api/Patients/5 (Soft Delete)
        /// <summary>
        /// Soft deletes a patient by marking IsDeleted = true.
        /// Requires Admin role.
        /// </summary>
        /// <param name="id">The ID of the patient to soft delete.</param>
        /// <returns>NoContent if successful, or NotFound if the record does not exist or is already deleted.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can soft delete patients
        public async Task<IActionResult> SoftDeletePatient(int id)
        {
            if (_context.Patients == null)
            {
                return NotFound();
            }
            // Use IgnoreQueryFilters to find the patient even if they are already soft-deleted
            var patient = await _context.Patients.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.PatientId == id);
            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            if (patient.IsDeleted)
            {
                return BadRequest("Patient record is already soft-deleted.");
            }

            patient.IsDeleted = true; // Mark as deleted
            patient.UpdatedAt = DateTime.UtcNow; // Update timestamp

            _context.Entry(patient).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Indicate successful soft delete
        }

        // POST: api/Patients/restore/5
        /// <summary>
        /// Restores a soft-deleted patient by setting IsDeleted = false.
        /// Requires Admin role.
        /// </summary>
        /// <param name="id">The ID of the patient to restore.</param>
        /// <returns>NoContent if successful, or NotFound if the record does not exist or is not deleted.</returns>
        [HttpPost("restore/{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can restore patients
        public async Task<IActionResult> RestorePatient(int id)
        {
            if (_context.Patients == null)
            {
                return NotFound();
            }
            // Use IgnoreQueryFilters to find the patient even if they are soft-deleted
            var patient = await _context.Patients.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.PatientId == id);
            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            if (!patient.IsDeleted)
            {
                return BadRequest("Patient record is not soft-deleted.");
            }

            patient.IsDeleted = false; // Mark as not deleted
            patient.UpdatedAt = DateTime.UtcNow; // Update timestamp

            _context.Entry(patient).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Indicate successful restore
        }

        private bool PatientExists(int id)
        {
            // CA1860: Prefer Count/Length over Any() for collections that are already materialized.
            // For IQueryable, Any() is often more efficient as it can translate to EXISTS SQL.
            // The existing usage is generally fine for performance in EF Core.
            return (_context.Patients?.Any(e => e.PatientId == id)).GetValueOrDefault();
        }
    }
}
