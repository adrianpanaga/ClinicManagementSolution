// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\StockTransactions\UpdateStockTransactionDto.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.StockTransactions
{
    public class UpdateStockTransactionDto
    {
        [Required(ErrorMessage = "Transaction ID is required for update.")]
        public int TransactionId { get; set; }

        public int? BatchId { get; set; } // Nullable for updates

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive value.")]
        public int? Quantity { get; set; } // Nullable for updates

        [StringLength(50, ErrorMessage = "Transaction type cannot exceed 50 characters.")]
        public string? TransactionType { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }

        public int? StaffId { get; set; }

        public int? PatientId { get; set; }
    }
}
