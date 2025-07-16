// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\ItemBatches\UpdateItemBatchDto.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.ItemBatches
{
    public class UpdateItemBatchDto
    {
        [Required(ErrorMessage = "Batch ID is required for update.")]
        public int BatchId { get; set; }

        public int? ItemId { get; set; } // Nullable for updates, as it might not always be changed

        [StringLength(100, ErrorMessage = "Batch number cannot exceed 100 characters.")]
        public string? BatchNumber { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative value.")]
        public int? Quantity { get; set; } // Nullable for updates, as it might not always be changed

        public DateTime? ExpirationDate { get; set; }

        public DateTime? ReceivedDate { get; set; }

        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Cost per unit must be a positive value.")]
        public decimal? CostPerUnit { get; set; }

        public int? VendorId { get; set; }
    }
}
