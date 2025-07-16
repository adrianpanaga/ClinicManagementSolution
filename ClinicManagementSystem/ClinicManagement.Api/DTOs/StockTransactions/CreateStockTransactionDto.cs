// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\StockTransactions\CreateStockTransactionDto.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.StockTransactions
{
    public class CreateStockTransactionDto
    {
        [Required(ErrorMessage = "Batch ID is required.")]
        public int BatchId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive value.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Transaction type is required.")]
        [StringLength(50, ErrorMessage = "Transaction type cannot exceed 50 characters.")]
        public string TransactionType { get; set; } = null!; // e.g., "IN", "OUT", "ADJUSTMENT"

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }

        public int? StaffId { get; set; } // Optional: Staff member initiating the transaction

        public int? PatientId { get; set; } // Optional: Patient receiving the item
    }
}
