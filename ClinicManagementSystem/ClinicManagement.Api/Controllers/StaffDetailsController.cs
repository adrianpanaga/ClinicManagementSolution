// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\Controllers\StaffDetailsController.cs

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
using ClinicManagement.Api.DTOs.StaffDetails; // Your StaffDetail DTOs

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All actions in this controller require an authenticated user by default
    public class StaffDetailsController : ControllerBase
    {
        private readonly ClinicManagementDbContext _context;

        public StaffDetailsController(ClinicManagementDbContext context)
        {
            _context = context;
        }

        // Helper method to map StaffDetail model to StaffDetailDto
        // CA1822: Mark method as static as it does not access instance data
        private static StaffDetailDto MapToStaffDetailDto(StaffDetail staffDetail)
        {
            if (staffDetail == null) return null; // CS8603: Possible null reference return. (Expected)

            return new StaffDetailDto
            {
                StaffId = staffDetail.StaffId,
                FirstName = staffDetail.FirstName,
                MiddleName = staffDetail.MiddleName,
                LastName = staffDetail.LastName,
                JobTitle = staffDetail.JobTitle,
                Specialization = staffDetail.Specialization,
                ContactNumber = staffDetail.ContactNumber,
                Email = staffDetail.Email,
                CreatedAt = staffDetail.CreatedAt,
                UpdatedAt = staffDetail.UpdatedAt,
                UserId = staffDetail.UserId,
                IsDeleted = staffDetail.IsDeleted // Include soft delete status
            };
        }

        // GET: api/StaffDetails
        /// <summary>
        /// Retrieves all staff details. For Admins/HR, can optionally include deleted records.
        /// </summary>
        /// <param name="includeDeleted">Optional. If true and user is Admin/HR, includes soft-deleted staff.</param>
        /// <returns>A list of StaffDetailDto.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,HR")] // Only Admin and HR can view all staff details
        public async Task<ActionResult<IEnumerable<StaffDetailDto>>> GetStaffDetails([FromQuery] bool includeDeleted = false)
        {
            if (_context.StaffDetails == null)
            {
                return NotFound();
            }

            IQueryable<StaffDetail> query = _context.StaffDetails;

            // Admins and HR can choose to see deleted staff details
            if (includeDeleted && (User.IsInRole("Admin") || User.IsInRole("HR")))
            {
                query = query.IgnoreQueryFilters(); // Bypass the global query filter
            }

            var staffDetails = await query.ToListAsync();
            return staffDetails.Select(s => MapToStaffDetailDto(s)).ToList();
        }

        // GET: api/StaffDetails/5
        /// <summary>
        /// Retrieves a specific staff detail by ID.
        /// Requires Admin, HR, or the specific Staff member (for their own record).
        /// Admins/HR can retrieve deleted records by ID.
        /// </summary>
        /// <param name="id">The ID of the staff member.</param>
        /// <returns>The StaffDetailDto if found, otherwise NotFound.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,HR,Doctor,Nurse,Receptionist,InventoryManager")] // Broad access, then filter
        public async Task<ActionResult<StaffDetailDto>> GetStaffDetail(int id)
        {
            if (_context.StaffDetails == null)
            {
                return NotFound();
            }

            IQueryable<StaffDetail> query = _context.StaffDetails;

            // Admins/HR can retrieve deleted records by ID
            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                query = query.IgnoreQueryFilters();
            }

            var staffDetail = await query.FirstOrDefaultAsync(s => s.StaffId == id);

            if (staffDetail == null)
            {
                return NotFound();
            }

            // Custom authorization for staff: Must match their own record
            if (!(User.IsInRole("Admin") || User.IsInRole("HR")))
            {
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized("Could not identify current user.");
                }

                // If StaffDetail.UserId is nullable, ensure it's not null before comparing
                if (!staffDetail.UserId.HasValue || staffDetail.UserId.Value != currentUserId)
                {
                    return Forbid("You do not have access to this staff record.");
                }
            }

            return MapToStaffDetailDto(staffDetail);
        }

        /// <summary>
        /// Gets a list of doctors available for public booking.
        /// This endpoint does NOT require authentication.
        /// </summary>
        /// <returns>A list of DoctorForBookingDto.</returns>
        [HttpGet("ForBooking")] // Route: GET /api/StaffDetails/ForBooking
        [AllowAnonymous] // IMPORTANT: Allows unauthenticated access
        public async Task<ActionResult<IEnumerable<DoctorForBookingDto>>> GetDoctorsForBooking()
        {
            var doctors = await _context.StaffDetails
                .Where(s => !string.IsNullOrEmpty(s.Specialization) && !s.IsDeleted) // Filter for specialists (doctors) and not deleted
                .Select(s => new DoctorForBookingDto
                {
                    StaffId = s.StaffId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    JobTitle = s.JobTitle,
                    Specialization = s.Specialization
                })
                .ToListAsync();

            return Ok(doctors);
        }

        // PUT: api/StaffDetails/5
        /// <summary>
        /// Updates an existing staff detail record.
        /// Requires Admin or HR role. A staff member might be able to update their own contact info.
        /// </summary>
        /// <param name="id">The ID of the staff member to update.</param>
        /// <param name="updateStaffDetailDto">The DTO object with updated data.</param>
        /// <returns>NoContent if successful, BadRequest if ID mismatch, NotFound if staff not found, or throws exception for concurrency.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,HR,Doctor,Nurse,Receptionist,InventoryManager")] // Allow staff to update their own basic info
        public async Task<IActionResult> PutStaffDetail(int id, UpdateStaffDetailDto updateStaffDetailDto)
        {
            if (id != updateStaffDetailDto.StaffId)
            {
                return BadRequest("Mismatched Staff ID in route and body.");
            }

            // Get the staff detail, ignoring global query filters so we can update even if soft-deleted
            var staffDetail = await _context.StaffDetails.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.StaffId == id);
            if (staffDetail == null)
            {
                return NotFound();
            }

            // Fine-grained authorization: Only Admin/HR can update any record, others only their own (non-sensitive fields)
            if (!(User.IsInRole("Admin") || User.IsInRole("HR")))
            {
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized("Could not identify current user.");
                }
                if (!staffDetail.UserId.HasValue || staffDetail.UserId.Value != currentUserId)
                {
                    return Forbid("You are not authorized to update this staff record.");
                }

                // If a non-Admin/HR user is updating their own record, restrict sensitive field updates
                if (!string.IsNullOrWhiteSpace(updateStaffDetailDto.JobTitle) && updateStaffDetailDto.JobTitle != staffDetail.JobTitle)
                {
                    return Forbid("You cannot change your job title.");
                }
                if (!string.IsNullOrWhiteSpace(updateStaffDetailDto.Specialization) && updateStaffDetailDto.Specialization != staffDetail.Specialization)
                {
                    return Forbid("You cannot change your specialization.");
                }
            }


            // Manually map properties from DTO to the existing EF Core entity
            staffDetail.FirstName = updateStaffDetailDto.FirstName ?? staffDetail.FirstName;
            staffDetail.MiddleName = updateStaffDetailDto.MiddleName ?? staffDetail.MiddleName;
            staffDetail.LastName = updateStaffDetailDto.LastName ?? staffDetail.LastName;
            staffDetail.JobTitle = updateStaffDetailDto.JobTitle ?? staffDetail.JobTitle;
            staffDetail.Specialization = updateStaffDetailDto.Specialization ?? staffDetail.Specialization;
            staffDetail.ContactNumber = updateStaffDetailDto.ContactNumber ?? staffDetail.ContactNumber;
            staffDetail.Email = updateStaffDetailDto.Email ?? staffDetail.Email;
            staffDetail.UpdatedAt = DateTime.UtcNow; // Set update timestamp

            _context.Entry(staffDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StaffDetailExists(id)) // Check existence, considering global filter.
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

        // POST: api/StaffDetails
        /// <summary>
        /// Creates a new staff detail record.
        /// Requires Admin or HR role.
        /// </summary>
        /// <param name="createStaffDetailDto">The DTO object to create.</param>
        /// <returns>The created StaffDetailDto with its ID, or a problem if the DbSet is null.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,HR")] // Only Admin or HR can create new staff records
        public async Task<ActionResult<StaffDetailDto>> PostStaffDetail(CreateStaffDetailDto createStaffDetailDto)
        {
            if (_context.StaffDetails == null)
            {
                return Problem("Entity set 'ClinicManagementDbContext.StaffDetails' is null.");
            }

            // UserId should typically be linked during user registration or by an admin, not directly in creation DTO
            // Removed direct UserId assignment from DTO as per your DTO design.
            // If UserId needs to be associated here, it must be part of CreateStaffDetailDto.
            // If UserId comes from the logged-in user, you would retrieve it from claims.
            // For now, new staff created via this endpoint will have UserId as NULL.

            // Manually map DTO to EF Core model
            var staffDetail = new StaffDetail
            {
                FirstName = createStaffDetailDto.FirstName!,
                MiddleName = createStaffDetailDto.MiddleName,
                LastName = createStaffDetailDto.LastName!,
                JobTitle = createStaffDetailDto.JobTitle!,
                Specialization = createStaffDetailDto.Specialization,
                ContactNumber = createStaffDetailDto.ContactNumber!,
                Email = createStaffDetailDto.Email!,
                CreatedAt = DateTime.UtcNow, // Set creation timestamp
                UpdatedAt = DateTime.UtcNow,  // Set initial update timestamp
                UserId = null, // Explicitly set to null, as it's not provided by the DTO.
                               // This link must be established separately if desired.
                IsDeleted = false // Default to not deleted
            };

            _context.StaffDetails.Add(staffDetail);
            await _context.SaveChangesAsync();

            // No need to load User here as StaffDetailDto only exposes UserId, which will be null if not linked.
            // If UserId needs to be fetched from a newly registered user account (created elsewhere),
            // this logic would be in a different place (e.g., AuthController's registration).

            return CreatedAtAction("GetStaffDetail", new { id = staffDetail.StaffId }, MapToStaffDetailDto(staffDetail));
        }

        // DELETE: api/StaffDetails/5 (Soft Delete)
        /// <summary>
        /// Soft deletes a staff detail by marking IsDeleted = true.
        /// Requires Admin role.
        /// </summary>
        /// <param name="id">The ID of the staff member to soft delete.</param>
        /// <returns>NoContent if successful, or NotFound if the record does not exist or is already deleted.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can soft delete staff details
        public async Task<IActionResult> SoftDeleteStaffDetail(int id)
        {
            if (_context.StaffDetails == null)
            {
                return NotFound();
            }
            // Use IgnoreQueryFilters to find the record even if it's already soft-deleted
            var staffDetail = await _context.StaffDetails.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.StaffId == id);
            if (staffDetail == null)
            {
                return NotFound("Staff detail not found.");
            }

            if (staffDetail.IsDeleted)
            {
                return BadRequest("Staff detail is already soft-deleted.");
            }

            staffDetail.IsDeleted = true; // Mark as deleted
            staffDetail.UpdatedAt = DateTime.UtcNow; // Update timestamp

            _context.Entry(staffDetail).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Indicate successful soft delete
        }

        // POST: api/StaffDetails/restore/5
        /// <summary>
        /// Restores a soft-deleted staff detail by setting IsDeleted = false.
        /// Requires Admin role.
        /// </summary>
        /// <param name="id">The ID of the staff member to restore.</param>
        /// <returns>NoContent if successful, or NotFound if the record does not exist or is not deleted.</returns>
        [HttpPost("restore/{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can restore staff details
        public async Task<IActionResult> RestoreStaffDetail(int id)
        {
            if (_context.StaffDetails == null)
            {
                return NotFound();
            }
            // Use IgnoreQueryFilters to find the record even if it's soft-deleted
            var staffDetail = await _context.StaffDetails.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.StaffId == id);
            if (staffDetail == null)
            {
                return NotFound("Staff detail not found.");
            }

            if (!staffDetail.IsDeleted)
            {
                return BadRequest("Staff detail is not soft-deleted.");
            }

            staffDetail.IsDeleted = false; // Mark as not deleted
            staffDetail.UpdatedAt = DateTime.UtcNow; // Update timestamp

            _context.Entry(staffDetail).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Indicate successful restore
        }

        private bool StaffDetailExists(int id)
        {
            // This helper checks for *active* records due to the global query filter.
            // If you need to check for existence including soft-deleted, use .IgnoreQueryFilters() here.
            return (_context.StaffDetails?.Any(e => e.StaffId == id)).GetValueOrDefault();
        }
    }
}
