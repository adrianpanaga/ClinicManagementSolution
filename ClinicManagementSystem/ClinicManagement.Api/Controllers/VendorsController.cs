// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\Controllers\VendorsController.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // For authorization attributes

using ClinicManagement.Data.Context; // Your DbContext
using ClinicManagement.Data.Models;   // Your EF Core models
using ClinicManagement.Api.DTOs.Vendors; // Your Vendor DTOs

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,InventoryManager")] // Only Admins and InventoryManagers can manage vendors
    public class VendorsController : ControllerBase
    {
        private readonly ClinicManagementDbContext _context;

        public VendorsController(ClinicManagementDbContext context)
        {
            _context = context;
        }

        // Helper method to map Vendor model to VendorDto
        private static VendorDto MapToVendorDto(Vendor vendor)
        {
            if (vendor == null) return null; // CS8603: Expected null return.

            return new VendorDto
            {
                VendorId = vendor.VendorId,
                VendorName = vendor.VendorName,
                ContactPerson = vendor.ContactPerson, // Added
                ContactNumber = vendor.ContactNumber, // Added
                Email = vendor.Email,                 // Added
                Address = vendor.Address,             // Added
                Notes = vendor.Notes,
                CreatedAt = vendor.CreatedAt,
                UpdatedAt = vendor.UpdatedAt,
                IsDeleted = vendor.IsDeleted // Include soft delete status
            };
        }

        // GET: api/Vendors
        /// <summary>
        /// Retrieves all vendors. Admins/InventoryManagers can optionally include deleted records.
        /// </summary>
        /// <param name="includeDeleted">Optional. If true, includes soft-deleted vendors.</param>
        /// <returns>A list of VendorDto.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VendorDto>>> GetVendors([FromQuery] bool includeDeleted = false)
        {
            if (_context.Vendors == null)
            {
                return NotFound();
            }

            IQueryable<Vendor> query = _context.Vendors;

            // Only include deleted if explicitly requested and allowed by role
            if (includeDeleted && (User.IsInRole("Admin") || User.IsInRole("InventoryManager")))
            {
                query = query.IgnoreQueryFilters(); // Bypass the global query filter
            }

            var vendors = await query.ToListAsync();
            return vendors.Select(v => MapToVendorDto(v)).ToList();
        }

        // GET: api/Vendors/5
        /// <summary>
        /// Retrieves a specific vendor by ID. Admins/InventoryManagers can retrieve deleted records by ID.
        /// </summary>
        /// <param name="id">The ID of the vendor.</param>
        /// <returns>The VendorDto if found, otherwise NotFound.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<VendorDto>> GetVendor(int id)
        {
            if (_context.Vendors == null)
            {
                return NotFound();
            }

            IQueryable<Vendor> query = _context.Vendors;

            // Admins/InventoryManagers can retrieve deleted records by ID
            if (User.IsInRole("Admin") || User.IsInRole("InventoryManager"))
            {
                query = query.IgnoreQueryFilters();
            }

            var vendor = await query.FirstOrDefaultAsync(v => v.VendorId == id);

            if (vendor == null)
            {
                return NotFound();
            }

            return MapToVendorDto(vendor);
        }

        // PUT: api/Vendors/5
        /// <summary>
        /// Updates an existing vendor record.
        /// </summary>
        /// <param name="id">The ID of the vendor to update.</param>
        /// <param name="updateVendorDto">The DTO object with updated data.</param>
        /// <returns>NoContent if successful, BadRequest if ID mismatch, NotFound if vendor not found, or throws exception for concurrency.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVendor(int id, UpdateVendorDto updateVendorDto)
        {
            if (id != updateVendorDto.VendorId)
            {
                return BadRequest("Mismatched Vendor ID in route and body.");
            }

            // Get the vendor, ignoring global query filters so we can update even if soft-deleted
            var vendor = await _context.Vendors.IgnoreQueryFilters().FirstOrDefaultAsync(v => v.VendorId == id);
            if (vendor == null)
            {
                return NotFound();
            }

            // Manually map properties from DTO to the existing EF Core entity
            vendor.VendorName = updateVendorDto.VendorName ?? vendor.VendorName;
            vendor.ContactPerson = updateVendorDto.ContactPerson ?? vendor.ContactPerson; // Added
            vendor.ContactNumber = updateVendorDto.ContactNumber ?? vendor.ContactNumber; // Added
            vendor.Email = updateVendorDto.Email ?? vendor.Email;                         // Added
            vendor.Address = updateVendorDto.Address ?? vendor.Address;                   // Added
            vendor.Notes = updateVendorDto.Notes ?? vendor.Notes;
            vendor.UpdatedAt = DateTime.UtcNow; // Set update timestamp

            _context.Entry(vendor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VendorExists(id)) // Check existence, considering global filter.
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

        // POST: api/Vendors
        /// <summary>
        /// Creates a new vendor record.
        /// </summary>
        /// <param name="createVendorDto">The DTO object to create.</param>
        /// <returns>The created VendorDto with its ID, or a problem if the DbSet is null.</returns>
        [HttpPost]
        public async Task<ActionResult<VendorDto>> PostVendor(CreateVendorDto createVendorDto)
        {
            if (_context.Vendors == null)
            {
                return Problem("Entity set 'ClinicManagementDbContext.Vendors' is null.");
            }

            // Manually map DTO to EF Core model
            var vendor = new Vendor
            {
                VendorName = createVendorDto.VendorName!,
                ContactPerson = createVendorDto.ContactPerson, // Added
                ContactNumber = createVendorDto.ContactNumber, // Added
                Email = createVendorDto.Email,                 // Added
                Address = createVendorDto.Address,             // Added
                Notes = createVendorDto.Notes,
                CreatedAt = DateTime.UtcNow, // Set creation timestamp
                UpdatedAt = DateTime.UtcNow,  // Set initial update timestamp
                IsDeleted = false // Default to not deleted
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVendor", new { id = vendor.VendorId }, MapToVendorDto(vendor));
        }

        // DELETE: api/Vendors/5 (Soft Delete)
        /// <summary>
        /// Soft deletes a vendor by marking IsDeleted = true.
        /// Requires Admin or InventoryManager role.
        /// </summary>
        /// <param name="id">The ID of the vendor to soft delete.</param>
        /// <returns>NoContent if successful, or NotFound if the record does not exist or is already deleted.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteVendor(int id)
        {
            if (_context.Vendors == null)
            {
                return NotFound();
            }
            // Use IgnoreQueryFilters to find the vendor even if they are already soft-deleted
            var vendor = await _context.Vendors.IgnoreQueryFilters().FirstOrDefaultAsync(v => v.VendorId == id);
            if (vendor == null)
            {
                return NotFound("Vendor not found.");
            }

            if (vendor.IsDeleted)
            {
                return BadRequest("Vendor record is already soft-deleted.");
            }

            vendor.IsDeleted = true; // Mark as deleted
            vendor.UpdatedAt = DateTime.UtcNow; // Update timestamp

            _context.Entry(vendor).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Indicate successful soft delete
        }

        // POST: api/Vendors/restore/5
        /// <summary>
        /// Restores a soft-deleted vendor by setting IsDeleted = false.
        /// Requires Admin or InventoryManager role.
        /// </summary>
        /// <param name="id">The ID of the vendor to restore.</param>
        /// <returns>NoContent if successful, or NotFound if the record does not exist or is not deleted.</returns>
        [HttpPost("restore/{id}")]
        public async Task<IActionResult> RestoreVendor(int id)
        {
            if (_context.Vendors == null)
            {
                return NotFound();
            }
            // Use IgnoreQueryFilters to find the vendor even if they are soft-deleted
            var vendor = await _context.Vendors.IgnoreQueryFilters().FirstOrDefaultAsync(v => v.VendorId == id);
            if (vendor == null)
            {
                return NotFound("Vendor not found.");
            }

            if (!vendor.IsDeleted)
            {
                return BadRequest("Vendor record is not soft-deleted.");
            }

            vendor.IsDeleted = false; // Mark as not deleted
            vendor.UpdatedAt = DateTime.UtcNow; // Update timestamp

            _context.Entry(vendor).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Indicate successful restore
        }

        private bool VendorExists(int id)
        {
            // This helper checks for *active* records due to the global query filter.
            // If you need to check for existence including soft-deleted, use .IgnoreQueryFilters() here.
            return (_context.Vendors?.Any(e => e.VendorId == id)).GetValueOrDefault();
        }
    }
}
