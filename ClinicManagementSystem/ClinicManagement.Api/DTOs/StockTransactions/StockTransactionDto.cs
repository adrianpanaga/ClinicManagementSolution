// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\StockTransactions\StockTransactionDto.cs

using System;
using ClinicManagement.Api.DTOs.ItemBatches;   // For ItemBatchDto
using ClinicManagement.Api.DTOs.Patients;     // For PatientDetailsDto (or PatientDto)
using ClinicManagement.Api.DTOs.StaffDetails; // For StaffDetailDto

namespace ClinicManagement.Api.DTOs.StockTransactions
{
    public class StockTransactionDto
    {
        public int TransactionId { get; set; }
        public int BatchId { get; set; }
        public int Quantity { get; set; }
        public string TransactionType { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime TransactionDate { get; set; }
        public int? StaffId { get; set; } // Nullable, as per model
        public int? PatientId { get; set; } // Nullable, as per model
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties (DTOs for related entities)
        public ItemBatchDto? Batch { get; set; }
        public StaffDetailDto? Staff { get; set; }
        public PatientDetailsDto? Patient { get; set; } // Assuming your Patient DTO is PatientDetailsDto
    }
}
