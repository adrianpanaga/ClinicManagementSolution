// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\Controllers\InventoryItemsController.cs

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
using ClinicManagement.Api.DTOs.InventoryItems; // Your InventoryItem DTOs

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All actions in this controller require an authenticated user by default
    public class InventoryItemsController : ControllerBase
    {
        private readonly ClinicManagementDbContext _context;

        public InventoryItemsController(ClinicManagementDbContext context)
        {
            _context = context;
        }

        // Helper method to manually map InventoryItem model to InventoryItemDto
        private InventoryItemDto MapToInventoryItemDto(InventoryItem item)
        {
            if (item == null) return null;

            return new InventoryItemDto
            {
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                Category = item.Category,
                UnitOfMeasure = item.UnitOfMeasure,
                PurchasePrice = item.PurchasePrice,
                SellingPrice = item.SellingPrice,
                ReorderLevel = item.ReorderLevel,
                LeadTimeDays = item.LeadTimeDays,
                Description = item.Description,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
                IsDeleted = item.IsDeleted,
                VendorId = item.VendorId // Assuming you want to include VendorId in the DTO
            };
        }

        // GET: api/InventoryItems
        /// <summary>
        /// Retrieves all inventory items. For Admins/Inventory Managers, can optionally include deleted records.
        /// </summary>
        /// <param name="includeDeleted">Optional. If true and user is Admin/InventoryManager, includes soft-deleted items.</param>
        /// <returns>A list of InventoryItemDto.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,InventoryManager")] // Roles allowed to view all inventory items
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetInventoryItems([FromQuery] bool includeDeleted = false)
        {
            if (_context.InventoryItems == null)
            {
                return NotFound();
            }

            IQueryable<InventoryItem> query = _context.InventoryItems;

            // Admins and Inventory Managers can choose to see deleted inventory items
            if (includeDeleted && (User.IsInRole("Admin") || User.IsInRole("InventoryManager")))
            {
                query = query.IgnoreQueryFilters(); // Bypass the global query filter
            }

            var items = await query.ToListAsync();
            return items.Select(item => MapToInventoryItemDto(item)).ToList();
        }

        // GET: api/InventoryItems/5
        /// <summary>
        /// Retrieves a specific inventory item by ID.
        /// Requires Admin or InventoryManager role. Admins/Inventory Managers can retrieve deleted records by ID.
        /// </summary>
        /// <param name="id">The ID of the inventory item.</param>
        /// <returns>The InventoryItemDto if found, otherwise NotFound.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<ActionResult<InventoryItemDto>> GetInventoryItem(int id)
        {
            if (_context.InventoryItems == null)
            {
                return NotFound();
            }

            IQueryable<InventoryItem> query = _context.InventoryItems;

            // Admins/Inventory Managers can retrieve deleted records by ID
            if (User.IsInRole("Admin") || User.IsInRole("InventoryManager"))
            {
                query = query.IgnoreQueryFilters();
            }

            var item = await query.FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null)
            {
                return NotFound();
            }

            return MapToInventoryItemDto(item);
        }

        // PUT: api/InventoryItems/5
        /// <summary>
        /// Updates an existing inventory item record.
        /// Requires Admin or InventoryManager role.
        /// </summary>
        /// <param name="id">The ID of the inventory item to update.</param>
        /// <param name="updateInventoryItemDto">The DTO object with updated data.</param>
        /// <returns>NoContent if successful, BadRequest if ID mismatch, NotFound if item not found, or throws exception for concurrency.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,InventoryManager")] // Roles allowed to update inventory items
        public async Task<IActionResult> PutInventoryItem(int id, UpdateInventoryItemDto updateInventoryItemDto)
        {
            if (id != updateInventoryItemDto.ItemId)
            {
                return BadRequest("Mismatched Item ID in route and body.");
            }

            // Get the item, ignoring global query filters so we can update even if soft-deleted
            var item = await _context.InventoryItems.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.ItemId == id);
            if (item == null)
            {
                return NotFound();
            }

            // Manually map properties from DTO to the existing EF Core entity
            item.ItemName = updateInventoryItemDto.ItemName ?? item.ItemName;
            item.Category = updateInventoryItemDto.Category ?? item.Category;
            item.UnitOfMeasure = updateInventoryItemDto.UnitOfMeasure ?? item.UnitOfMeasure;
            item.PurchasePrice = updateInventoryItemDto.PurchasePrice ?? item.PurchasePrice;
            item.SellingPrice = updateInventoryItemDto.SellingPrice ?? item.SellingPrice;
            item.ReorderLevel = updateInventoryItemDto.ReorderLevel ?? item.ReorderLevel;
            item.LeadTimeDays = updateInventoryItemDto.LeadTimeDays ?? item.LeadTimeDays;
            item.Description = updateInventoryItemDto.Description ?? item.Description;
            item.UpdatedAt = DateTime.UtcNow; // Set update timestamp
            item.VendorId = updateInventoryItemDto.VendorId; // Assuming VendorId is always required

            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryItemExists(id)) // Check existence, considering global filter for active records
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

        // POST: api/InventoryItems
        /// <summary>
        /// Creates a new inventory item record.
        /// Requires Admin or InventoryManager role.
        /// </summary>
        /// <param name="createInventoryItemDto">The DTO object to create.</param>
        /// <returns>The created InventoryItemDto with its ID, or a problem if the DbSet is null.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,InventoryManager")] // Only Admin or Inventory Manager can create new inventory item records
        public async Task<ActionResult<InventoryItemDto>> PostInventoryItem(CreateInventoryItemDto createInventoryItemDto)
        {
            if (_context.InventoryItems == null)
            {
                return Problem("Entity set 'ClinicManagementDbContext.InventoryItems' is null.");
            }

            // Manually map DTO to EF Core model
            var item = new InventoryItem
            {
                ItemName = createInventoryItemDto.ItemName,
                Category = createInventoryItemDto.Category,
                UnitOfMeasure = createInventoryItemDto.UnitOfMeasure,
                PurchasePrice = createInventoryItemDto.PurchasePrice,
                SellingPrice = createInventoryItemDto.SellingPrice,
                ReorderLevel = createInventoryItemDto.ReorderLevel,
                LeadTimeDays = createInventoryItemDto.LeadTimeDays,
                Description = createInventoryItemDto.Description,
                CreatedAt = DateTime.UtcNow, // Set creation timestamp
                UpdatedAt = DateTime.UtcNow,  // Set initial update timestamp
                IsDeleted = false, // Default to not deleted
                VendorId = createInventoryItemDto.VendorId // Assuming VendorId is always required
            };

            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInventoryItem", new { id = item.ItemId }, MapToInventoryItemDto(item));
        }

        // DELETE: api/InventoryItems/5 (Soft Delete)
        /// <summary>
        /// Soft deletes an inventory item by marking IsDeleted = true.
        /// Requires Admin or InventoryManager role.
        /// </summary>
        /// <param name="id">The ID of the inventory item to soft delete.</param>
        /// <returns>NoContent if successful, or NotFound if the record does not exist or is already deleted.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,InventoryManager")] // Roles allowed to soft delete inventory items
        public async Task<IActionResult> SoftDeleteInventoryItem(int id)
        {
            if (_context.InventoryItems == null)
            {
                return NotFound();
            }
            // Use IgnoreQueryFilters to find the record even if it's already soft-deleted
            var item = await _context.InventoryItems.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.ItemId == id);
            if (item == null)
            {
                return NotFound("Inventory item not found.");
            }

            if (item.IsDeleted)
            {
                return BadRequest("Inventory item is already soft-deleted.");
            }

            item.IsDeleted = true; // Mark as deleted
            item.UpdatedAt = DateTime.UtcNow; // Update timestamp

            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Indicate successful soft delete
        }

        // POST: api/InventoryItems/restore/5
        /// <summary>
        /// Restores a soft-deleted inventory item by setting IsDeleted = false.
        /// Requires Admin or InventoryManager role.
        /// </summary>
        /// <param name="id">The ID of the inventory item to restore.</param>
        /// <returns>NoContent if successful, or NotFound if the record does not exist or is not deleted.</returns>
        [HttpPost("restore/{id}")]
        [Authorize(Roles = "Admin,InventoryManager")] // Roles allowed to restore inventory items
        public async Task<IActionResult> RestoreInventoryItem(int id)
        {
            if (_context.InventoryItems == null)
            {
                return NotFound();
            }
            // Use IgnoreQueryFilters to find the record even if it's soft-deleted
            var item = await _context.InventoryItems.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.ItemId == id);
            if (item == null)
            {
                return NotFound("Inventory item not found.");
            }

            if (!item.IsDeleted)
            {
                return BadRequest("Inventory item is not soft-deleted.");
            }

            item.IsDeleted = false; // Mark as not deleted
            item.UpdatedAt = DateTime.UtcNow; // Update timestamp

            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Indicate successful restore
        }

        private bool InventoryItemExists(int id)
        {
            return (_context.InventoryItems?.Any(e => e.ItemId == id)).GetValueOrDefault();
        }
    }
}
