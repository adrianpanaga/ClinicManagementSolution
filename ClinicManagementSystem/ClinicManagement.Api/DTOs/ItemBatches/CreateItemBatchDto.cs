// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\ItemBatches\CreateItemBatchDto.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.Api.DTOs.ItemBatches
{
    public class CreateItemBatchDto
    {
        [Required(ErrorMessage = "Item ID is required.")]
        public int? ItemId { get; set; } // Nullable, matches model (int?) but [Required] for DTO input

        [Required(ErrorMessage = "Batch number is required.")]
        [StringLength(100, ErrorMessage = "Batch number cannot exceed 100 characters.")]
        public string BatchNumber { get; set; } = null!;

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow.Date; // Default to current date if not provided

        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Cost per unit must be a positive value.")]
        public decimal? CostPerUnit { get; set; }

        public int? VendorId { get; set; }
    }
}
