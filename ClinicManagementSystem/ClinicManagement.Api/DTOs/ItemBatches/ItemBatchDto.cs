// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\ItemBatches\ItemBatchDto.cs

using System;
using ClinicManagement.Api.DTOs.InventoryItems; // For InventoryItemDto
using ClinicManagement.Api.DTOs.Vendors;       // For VendorDto

namespace ClinicManagement.Api.DTOs.ItemBatches
{
    public class ItemBatchDto
    {
        public int BatchId { get; set; }
        public int? ItemId { get; set; } // Nullable, as per model
        public string BatchNumber { get; set; } = null!;
        public int Quantity { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime ReceivedDate { get; set; }
        public decimal? CostPerUnit { get; set; }
        public int? VendorId { get; set; } // Nullable, as per model
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties (DTOs for related entities)
        public InventoryItemDto? Item { get; set; }
        public VendorDto? Vendor { get; set; }
    }
}
