// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\Controllers\ItemBatchesController.cs

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
using ClinicManagement.Api.DTOs.ItemBatches;    // Your ItemBatch DTOs
using ClinicManagement.Api.DTOs.InventoryItems; // For InventoryItemDto
using ClinicManagement.Api.DTOs.Vendors;       // For VendorDto

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,InventoryManager")] // All actions in this controller require these roles by default
    public class ItemBatchesController : ControllerBase
    {
        private readonly ClinicManagementDbContext _context;

        public ItemBatchesController(ClinicManagementDbContext context)
        {
            _context = context;
        }

        // Helper method to manually map ItemBatch model to ItemBatchDto
        private ItemBatchDto MapToItemBatchDto(ItemBatch batch)
        {
            if (batch == null) return null;

            return new ItemBatchDto
            {
                BatchId = batch.BatchId,
                ItemId = batch.ItemId,
                BatchNumber = batch.BatchNumber,
                Quantity = batch.Quantity,
                ExpirationDate = batch.ExpirationDate,
                ReceivedDate = batch.ReceivedDate,
                CostPerUnit = batch.CostPerUnit,
                VendorId = batch.VendorId,
                CreatedAt = batch.CreatedAt,
                UpdatedAt = batch.UpdatedAt,
                // Manually map related entities to their respective DTOs
                Item = batch.Item != null ? new InventoryItemDto
                {
                    ItemId = batch.Item.ItemId,
                    ItemName = batch.Item.ItemName,
                    Category = batch.Item.Category,
                    UnitOfMeasure = batch.Item.UnitOfMeasure,
                    PurchasePrice = batch.Item.PurchasePrice,
                    SellingPrice = batch.Item.SellingPrice,
                    ReorderLevel = batch.Item.ReorderLevel,
                    LeadTimeDays = batch.Item.LeadTimeDays,
                    Description = batch.Item.Description,
                    CreatedAt = batch.Item.CreatedAt,
                    UpdatedAt = batch.Item.UpdatedAt,
                    IsDeleted = batch.Item.IsDeleted
                } : null,
                Vendor = batch.Vendor != null ? new VendorDto
                {
                    VendorId = batch.Vendor.VendorId,
                    VendorName = batch.Vendor.VendorName,
                    Address = batch.Vendor.Address,
                    ContactNumber = batch.Vendor.ContactNumber,
                    Email = batch.Vendor.Email,
                    Notes = batch.Vendor.Notes,
                    CreatedAt = batch.Vendor.CreatedAt,
                    UpdatedAt = batch.Vendor.UpdatedAt,
                    IsDeleted = batch.Vendor.IsDeleted
                } : null
            };
        }

        // GET: api/ItemBatches
        /// <summary>
        /// Retrieves all item batches, optionally including soft-deleted related entities.
        /// Requires Admin or InventoryManager role.
        /// </summary>
        /// <param name="includeDeletedRelated">Optional. If true, includes related soft-deleted InventoryItems and Vendors.</param>
        /// <returns>A list of ItemBatchDto.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemBatchDto>>> GetItemBatches([FromQuery] bool includeDeletedRelated = false)
        {
            if (_context.ItemBatches == null)
            {
                return NotFound();
            }

            IQueryable<ItemBatch> query = _context.ItemBatches;

            // Always include related entities. Global query filters for IsDeleted on InventoryItems and Vendors
            // will apply here, meaning soft-deleted related entities will *not* be loaded by default.
            query = query
                .Include(b => b.Item)
                .Include(b => b.Vendor);

            var batches = await query.ToListAsync();

            // If includeDeletedRelated is true, manually load any related items/vendors that were filtered out
            // by the global query filter (i.e., they are soft-deleted but referenced).
            if (includeDeletedRelated)
            {
                foreach (var batch in batches)
                {
                    // If Item was filtered out (null) but ItemId exists, try to load it ignoring filters
                    if (batch.Item == null && batch.ItemId.HasValue)
                    {
                        batch.Item = await _context.InventoryItems.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.ItemId == batch.ItemId.Value);
                    }
                    // If Vendor was filtered out (null) but VendorId exists, try to load it ignoring filters
                    if (batch.Vendor == null && batch.VendorId.HasValue)
                    {
                        batch.Vendor = await _context.Vendors.IgnoreQueryFilters().FirstOrDefaultAsync(v => v.VendorId == batch.VendorId.Value);
                    }
                }
            }

            return batches.Select(b => MapToItemBatchDto(b)).ToList();
        }


        // GET: api/ItemBatches/5
        /// <summary>
        /// Retrieves a specific item batch by ID, optionally including soft-deleted related entities.
        /// Requires Admin or InventoryManager role.
        /// </summary>
        /// <param name="id">The ID of the item batch.</param>
        /// <param name="includeDeletedRelated">Optional. If true, includes related soft-deleted InventoryItems and Vendors.</param>
        /// <returns>The ItemBatchDto if found, otherwise NotFound.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemBatchDto>> GetItemBatch(int id, [FromQuery] bool includeDeletedRelated = false)
        {
            if (_context.ItemBatches == null)
            {
                return NotFound();
            }

            // Always include related entities. Global query filters for IsDeleted on InventoryItems and Vendors
            // will apply here, meaning soft-deleted related entities will *not* be loaded by default.
            var batch = await _context.ItemBatches
                                      .Include(b => b.Item)
                                      .Include(b => b.Vendor)
                                      .FirstOrDefaultAsync(b => b.BatchId == id);

            if (batch == null)
            {
                return NotFound();
            }

            // If includeDeletedRelated is true, manually load any related items/vendors that were filtered out
            // by the global query filter (i.e., they are soft-deleted but referenced).
            if (includeDeletedRelated)
            {
                // If Item was filtered out (null) but ItemId exists, try to load it ignoring filters
                if (batch.Item == null && batch.ItemId.HasValue)
                {
                    batch.Item = await _context.InventoryItems.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.ItemId == batch.ItemId.Value);
                }
                // If Vendor was filtered out (null) but VendorId exists, try to load it ignoring filters
                if (batch.Vendor == null && batch.VendorId.HasValue)
                {
                    batch.Vendor = await _context.Vendors.IgnoreQueryFilters().FirstOrDefaultAsync(v => v.VendorId == batch.VendorId.Value);
                }
            }

            return MapToItemBatchDto(batch);
        }

        // POST: api/ItemBatches
        /// <summary>
        /// Creates a new item batch record.
        /// Requires Admin or InventoryManager role.
        /// </summary>
        /// <param name="createItemBatchDto">The DTO object to create.</param>
        /// <returns>The created ItemBatchDto with its ID, or a problem if the DbSet is null.</returns>
        [HttpPost]
        public async Task<ActionResult<ItemBatchDto>> PostItemBatch(CreateItemBatchDto createItemBatchDto)
        {
            if (_context.ItemBatches == null)
            {
                return Problem("Entity set 'ClinicManagementDbContext.ItemBatches' is null.");
            }

            // Validate ItemId and VendorId exist (and are not soft-deleted by default)
            if (createItemBatchDto.ItemId.HasValue)
            {
                var itemExists = await _context.InventoryItems.AnyAsync(i => i.ItemId == createItemBatchDto.ItemId.Value && !i.IsDeleted);
                if (!itemExists)
                {
                    return BadRequest($"InventoryItem with ID {createItemBatchDto.ItemId.Value} does not exist or is deleted.");
                }
            }

            if (createItemBatchDto.VendorId.HasValue)
            {
                var vendorExists = await _context.Vendors.AnyAsync(v => v.VendorId == createItemBatchDto.VendorId.Value && !v.IsDeleted);
                if (!vendorExists)
                {
                    return BadRequest($"Vendor with ID {createItemBatchDto.VendorId.Value} does not exist or is deleted.");
                }
            }


            // Manually map DTO to EF Core model
            var batch = new ItemBatch
            {
                ItemId = createItemBatchDto.ItemId,
                BatchNumber = createItemBatchDto.BatchNumber,
                Quantity = createItemBatchDto.Quantity,
                ExpirationDate = createItemBatchDto.ExpirationDate,
                ReceivedDate = createItemBatchDto.ReceivedDate,
                CostPerUnit = createItemBatchDto.CostPerUnit,
                VendorId = createItemBatchDto.VendorId,
                CreatedAt = DateTime.UtcNow, // Set creation timestamp
                UpdatedAt = DateTime.UtcNow   // Set initial update timestamp
            };

            _context.ItemBatches.Add(batch);
            await _context.SaveChangesAsync();

            // Reload related entities to map them into the DTO for the response
            await _context.Entry(batch).Reference(b => b.Item).LoadAsync();
            await _context.Entry(batch).Reference(b => b.Vendor).LoadAsync();

            return CreatedAtAction("GetItemBatch", new { id = batch.BatchId }, MapToItemBatchDto(batch));
        }

        // PUT: api/ItemBatches/5
        /// <summary>
        /// Updates an existing item batch record.
        /// Requires Admin or InventoryManager role.
        /// </summary>
        /// <param name="id">The ID of the item batch to update.</param>
        /// <param name="updateItemBatchDto">The DTO object with updated data.</param>
        /// <returns>NoContent if successful, BadRequest if ID mismatch, NotFound if batch not found, or throws exception for concurrency.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItemBatch(int id, UpdateItemBatchDto updateItemBatchDto)
        {
            if (id != updateItemBatchDto.BatchId)
            {
                return BadRequest("Mismatched Batch ID in route and body.");
            }

            var batch = await _context.ItemBatches.FindAsync(id);
            if (batch == null)
            {
                return NotFound();
            }

            // Validate ItemId and VendorId exist if they are being updated
            if (updateItemBatchDto.ItemId.HasValue && updateItemBatchDto.ItemId != batch.ItemId)
            {
                var itemExists = await _context.InventoryItems.AnyAsync(i => i.ItemId == updateItemBatchDto.ItemId.Value && !i.IsDeleted);
                if (!itemExists)
                {
                    return BadRequest($"InventoryItem with ID {updateItemBatchDto.ItemId.Value} does not exist or is deleted.");
                }
                batch.ItemId = updateItemBatchDto.ItemId;
            }

            if (updateItemBatchDto.VendorId.HasValue && updateItemBatchDto.VendorId != batch.VendorId)
            {
                var vendorExists = await _context.Vendors.AnyAsync(v => v.VendorId == updateItemBatchDto.VendorId.Value && !v.IsDeleted);
                if (!vendorExists)
                {
                    return BadRequest($"Vendor with ID {updateItemBatchDto.VendorId.Value} does not exist or is deleted.");
                }
                batch.VendorId = updateItemBatchDto.VendorId;
            }

            // Manually map properties from DTO to the existing EF Core entity
            batch.BatchNumber = updateItemBatchDto.BatchNumber ?? batch.BatchNumber;
            batch.Quantity = updateItemBatchDto.Quantity ?? batch.Quantity;
            batch.ExpirationDate = updateItemBatchDto.ExpirationDate ?? batch.ExpirationDate;
            batch.ReceivedDate = updateItemBatchDto.ReceivedDate ?? batch.ReceivedDate;
            batch.CostPerUnit = updateItemBatchDto.CostPerUnit ?? batch.CostPerUnit;
            batch.UpdatedAt = DateTime.UtcNow; // Set update timestamp

            _context.Entry(batch).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemBatchExists(id))
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

        // DELETE: api/ItemBatches/5 (Hard Delete)
        /// <summary>
        /// Deletes an item batch record. Note: This is a hard delete.
        /// Requires Admin or InventoryManager role.
        /// </summary>
        /// <param name="id">The ID of the item batch to delete.</param>
        /// <returns>NoContent if successful, or NotFound if the record does not exist.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemBatch(int id)
        {
            if (_context.ItemBatches == null)
            {
                return NotFound();
            }
            var batch = await _context.ItemBatches.FindAsync(id);
            if (batch == null)
            {
                return NotFound();
            }

            _context.ItemBatches.Remove(batch);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItemBatchExists(int id)
        {
            return (_context.ItemBatches?.Any(e => e.BatchId == id)).GetValueOrDefault();
        }
    }
}
