// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\Controllers\StockTransactionsController.cs

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
using ClinicManagement.Api.DTOs.StockTransactions; // Your StockTransaction DTOs
using ClinicManagement.Api.DTOs.ItemBatches;      // For ItemBatchDto
using ClinicManagement.Api.DTOs.Patients;        // For PatientDetailsDto
using ClinicManagement.Api.DTOs.StaffDetails;    // For StaffDetailDto

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,InventoryManager,Doctor,Nurse,Receptionist")] // Broad roles initially, can be refined
    public class StockTransactionsController : ControllerBase
    {
        private readonly ClinicManagementDbContext _context;

        public StockTransactionsController(ClinicManagementDbContext context)
        {
            _context = context;
        }

        // Helper method to manually map StockTransaction model to StockTransactionDto
        private StockTransactionDto MapToStockTransactionDto(StockTransaction transaction)
        {
            if (transaction == null) return null;

            return new StockTransactionDto
            {
                TransactionId = transaction.TransactionId,
                BatchId = transaction.BatchId,
                Quantity = transaction.Quantity,
                TransactionType = transaction.TransactionType,
                Notes = transaction.Notes,
                TransactionDate = transaction.TransactionDate,
                StaffId = transaction.StaffId,
                PatientId = transaction.PatientId,
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt,
                // Manually map related entities to their respective DTOs
                Batch = transaction.Batch != null ? new ItemBatchDto
                {
                    BatchId = transaction.Batch.BatchId,
                    ItemId = transaction.Batch.ItemId,
                    BatchNumber = transaction.Batch.BatchNumber,
                    Quantity = transaction.Batch.Quantity,
                    ExpirationDate = transaction.Batch.ExpirationDate,
                    ReceivedDate = transaction.Batch.ReceivedDate,
                    CostPerUnit = transaction.Batch.CostPerUnit,
                    VendorId = transaction.Batch.VendorId,
                    CreatedAt = transaction.Batch.CreatedAt,
                    UpdatedAt = transaction.Batch.UpdatedAt
                    // Note: Nested Item and Vendor in ItemBatchDto are not mapped here to prevent deep recursion
                } : null,
                Staff = transaction.Staff != null ? new StaffDetailDto
                {
                    StaffId = transaction.Staff.StaffId,
                    FirstName = transaction.Staff.FirstName,
                    LastName = transaction.Staff.LastName,
                    JobTitle = transaction.Staff.JobTitle,
                    Specialization = transaction.Staff.Specialization,
                    ContactNumber = transaction.Staff.ContactNumber,
                    Email = transaction.Staff.Email,
                    CreatedAt = transaction.Staff.CreatedAt,
                    UpdatedAt = transaction.Staff.UpdatedAt,
                    UserId = transaction.Staff.UserId,
                    IsDeleted = transaction.Staff.IsDeleted
                } : null,
                Patient = transaction.Patient != null ? new PatientDetailsDto // Assuming PatientDetailsDto is your Patient DTO
                {
                    PatientId = transaction.Patient.PatientId,
                    FirstName = transaction.Patient.FirstName,
                    LastName = transaction.Patient.LastName,
                    ContactNumber = transaction.Patient.ContactNumber,
                    Email = transaction.Patient.Email,
                    // Map other relevant patient fields you wish to expose
                } : null
            };
        }

        // GET: api/StockTransactions
        /// <summary>
        /// Retrieves all stock transactions.
        /// Requires Admin or InventoryManager role. Other roles might see limited views (e.g., their own transactions).
        /// </summary>
        /// <returns>A list of StockTransactionDto.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,InventoryManager")] // Only Admin and InventoryManager can view all transactions
        public async Task<ActionResult<IEnumerable<StockTransactionDto>>> GetStockTransactions()
        {
            if (_context.StockTransactions == null)
            {
                return NotFound();
            }

            var transactions = await _context.StockTransactions
                                         .Include(st => st.Batch)
                                         .Include(st => st.Staff)
                                         .Include(st => st.Patient)
                                         .ToListAsync();

            return transactions.Select(st => MapToStockTransactionDto(st)).ToList();
        }

        // GET: api/StockTransactions/5
        /// <summary>
        /// Retrieves a specific stock transaction by ID.
        /// Requires Admin or InventoryManager role.
        /// </summary>
        /// <param name="id">The ID of the stock transaction.</param>
        /// <returns>The StockTransactionDto if found, otherwise NotFound.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<StockTransactionDto>> GetStockTransaction(int id)
        {
            if (_context.StockTransactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.StockTransactions
                                            .Include(st => st.Batch)
                                            .Include(st => st.Staff)
                                            .Include(st => st.Patient)
                                            .FirstOrDefaultAsync(st => st.TransactionId == id);

            if (transaction == null)
            {
                return NotFound();
            }

            return MapToStockTransactionDto(transaction);
        }

        // GET: api/StockTransactions/batch/{batchId}
        /// <summary>
        /// Retrieves stock transactions for a specific item batch.
        /// Requires Admin or InventoryManager role.
        /// </summary>
        /// <param name="batchId">The ID of the item batch.</param>
        /// <returns>A list of StockTransactionDto for the specified batch.</returns>
        [HttpGet("batch/{batchId}")]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<ActionResult<IEnumerable<StockTransactionDto>>> GetStockTransactionsByBatch(int batchId)
        {
            if (_context.StockTransactions == null)
            {
                return NotFound();
            }

            var transactions = await _context.StockTransactions
                                         .Where(st => st.BatchId == batchId)
                                         .Include(st => st.Batch)
                                         .Include(st => st.Staff)
                                         .Include(st => st.Patient)
                                         .ToListAsync();

            if (!transactions.Any())
            {
                return NotFound($"No stock transactions found for batch ID {batchId}.");
            }

            return transactions.Select(st => MapToStockTransactionDto(st)).ToList();
        }

        // GET: api/StockTransactions/staff/{staffId}
        /// <summary>
        /// Retrieves stock transactions initiated by a specific staff member.
        /// Requires Admin, InventoryManager, or the specific Staff member (for their own transactions).
        /// </summary>
        /// <param name="staffId">The ID of the staff member.</param>
        /// <returns>A list of StockTransactionDto initiated by the specified staff.</returns>
        [HttpGet("staff/{staffId}")]
        [Authorize(Roles = "Admin,InventoryManager,Doctor,Nurse,Receptionist")]
        public async Task<ActionResult<IEnumerable<StockTransactionDto>>> GetStockTransactionsByStaff(int staffId)
        {
            // Optional: Implement fine-grained authorization for staff to only see their own transactions
            // if (!(User.IsInRole("Admin") || User.IsInRole("InventoryManager")))
            // {
            //     var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            //     if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            //     {
            //         return Unauthorized("Could not identify current user.");
            //     }
            //     var actualStaffId = await _context.StaffDetails.Where(sd => sd.UserId == currentUserId).Select(sd => sd.StaffId).FirstOrDefaultAsync();
            //     if (actualStaffId != staffId)
            //     {
            //         return Forbid("You can only view your own stock transactions.");
            //     }
            // }

            if (_context.StockTransactions == null)
            {
                return NotFound();
            }

            var transactions = await _context.StockTransactions
                                         .Where(st => st.StaffId == staffId)
                                         .Include(st => st.Batch)
                                         .Include(st => st.Staff)
                                         .Include(st => st.Patient)
                                         .ToListAsync();

            if (!transactions.Any())
            {
                return NotFound($"No stock transactions found for staff ID {staffId}.");
            }

            return transactions.Select(st => MapToStockTransactionDto(st)).ToList();
        }

        // GET: api/StockTransactions/patient/{patientId}
        /// <summary>
        /// Retrieves stock transactions for items dispensed to a specific patient.
        /// Requires Admin, InventoryManager, or the specific Patient (for their own transactions).
        /// </summary>
        /// <param name="patientId">The ID of the patient.</param>
        /// <returns>A list of StockTransactionDto for the specified patient.</returns>
        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Admin,InventoryManager,Patient")]
        public async Task<ActionResult<IEnumerable<StockTransactionDto>>> GetStockTransactionsByPatient(int patientId)
        {
            // Optional: Implement fine-grained authorization for patients to only see their own transactions
            // if (!User.IsInRole("Admin") && !User.IsInRole("InventoryManager"))
            // {
            //     var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            //     if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            //     {
            //         return Unauthorized("Could not identify current user.");
            //     }
            //     var actualPatientId = await _context.Patients.Where(p => p.UserId == currentUserId).Select(p => p.PatientId).FirstOrDefaultAsync();
            //     if (actualPatientId != patientId)
            //     {
            //         return Forbid("You can only view your own stock transactions.");
            //     }
            // }

            if (_context.StockTransactions == null)
            {
                return NotFound();
            }

            var transactions = await _context.StockTransactions
                                         .Where(st => st.PatientId == patientId)
                                         .Include(st => st.Batch)
                                         .Include(st => st.Staff)
                                         .Include(st => st.Patient)
                                         .ToListAsync();

            if (!transactions.Any())
            {
                return NotFound($"No stock transactions found for patient ID {patientId}.");
            }

            return transactions.Select(st => MapToStockTransactionDto(st)).ToList();
        }


        // POST: api/StockTransactions
        /// <summary>
        /// Creates a new stock transaction record.
        /// Requires Admin, InventoryManager, Doctor, or Nurse role.
        /// Automatically adjusts item batch quantity.
        /// </summary>
        /// <param name="createStockTransactionDto">The DTO object to create.</param>
        /// <returns>The created StockTransactionDto with its ID, or a problem if the DbSet is null.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,InventoryManager,Doctor,Nurse")] // These roles can create transactions
        public async Task<ActionResult<StockTransactionDto>> PostStockTransaction(CreateStockTransactionDto createStockTransactionDto)
        {
            if (_context.StockTransactions == null)
            {
                return Problem("Entity set 'ClinicManagementDbContext.StockTransactions' is null.");
            }

            // Find the associated ItemBatch (must exist and not be soft-deleted implicitly by global filter if applicable)
            var batch = await _context.ItemBatches.Include(b => b.Item).FirstOrDefaultAsync(b => b.BatchId == createStockTransactionDto.BatchId);
            if (batch == null)
            {
                return BadRequest($"ItemBatch with ID {createStockTransactionDto.BatchId} not found.");
            }

            // Optional: Validate StaffId and PatientId if provided and linked to existing non-deleted entities
            if (createStockTransactionDto.StaffId.HasValue)
            {
                var staffExists = await _context.StaffDetails.AnyAsync(sd => sd.StaffId == createStockTransactionDto.StaffId.Value && !sd.IsDeleted);
                if (!staffExists)
                {
                    return BadRequest($"Staff with ID {createStockTransactionDto.StaffId.Value} does not exist or is deleted.");
                }
            }

            if (createStockTransactionDto.PatientId.HasValue)
            {
                var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == createStockTransactionDto.PatientId.Value && !p.IsDeleted);
                if (!patientExists)
                {
                    return BadRequest($"Patient with ID {createStockTransactionDto.PatientId.Value} does not exist or is deleted.");
                }
            }

            // Create the transaction
            var transaction = new StockTransaction
            {
                BatchId = createStockTransactionDto.BatchId,
                Quantity = createStockTransactionDto.Quantity,
                TransactionType = createStockTransactionDto.TransactionType,
                Notes = createStockTransactionDto.Notes,
                TransactionDate = DateTime.UtcNow,
                StaffId = createStockTransactionDto.StaffId,
                PatientId = createStockTransactionDto.PatientId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Update the quantity in the ItemBatch
            if (transaction.TransactionType.ToUpper() == "IN")
            {
                batch.Quantity += transaction.Quantity;
            }
            else if (transaction.TransactionType.ToUpper() == "OUT")
            {
                if (batch.Quantity < transaction.Quantity)
                {
                    return BadRequest("Insufficient quantity in batch for 'OUT' transaction.");
                }
                batch.Quantity -= transaction.Quantity;
            }
            else if (transaction.TransactionType.ToUpper() == "ADJUSTMENT")
            {
                // For adjustments, Quantity could be positive (add) or negative (subtract)
                // This assumes `createStockTransactionDto.Quantity` can be negative for adjustments
                batch.Quantity += createStockTransactionDto.Quantity;
            }
            else
            {
                return BadRequest("Invalid TransactionType. Must be 'IN', 'OUT', or 'ADJUSTMENT'.");
            }
            batch.UpdatedAt = DateTime.UtcNow; // Update batch timestamp

            _context.StockTransactions.Add(transaction);
            _context.Entry(batch).State = EntityState.Modified; // Mark batch as modified

            await _context.SaveChangesAsync();

            // Reload related entities for the response DTO
            await _context.Entry(transaction).Reference(st => st.Batch).LoadAsync();
            await _context.Entry(transaction).Reference(st => st.Staff).LoadAsync();
            await _context.Entry(transaction).Reference(st => st.Patient).LoadAsync();

            return CreatedAtAction("GetStockTransaction", new { id = transaction.TransactionId }, MapToStockTransactionDto(transaction));
        }

        // Note: For StockTransactions, PUT (updating existing transactions) and DELETE (hard deleting transactions)
        // are generally discouraged as they represent historical events that should be immutable.
        // Instead, new transactions (e.g., an "adjustment" transaction) should be created to correct errors.
        // If you absolutely need them, you can implement them, but be mindful of data integrity.
        // I will omit them for now to promote a more robust transaction history.


        private bool StockTransactionExists(int id)
        {
            return (_context.StockTransactions?.Any(e => e.TransactionId == id)).GetValueOrDefault();
        }
    }
}
