// Location: C:\Users\AdrianPanaga\NewClinicApi\ClinicManagement.ApiNew\DTOs\InventoryItems\InventoryItemDto.cs

using System;

namespace ClinicManagement.Api.DTOs.InventoryItems
{
    public class InventoryItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = null!;
        public string? Category { get; set; }
        public string? UnitOfMeasure { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? SellingPrice { get; set; }
        public int? ReorderLevel { get; set; }
        public int? LeadTimeDays { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } // Include soft delete status
        public int VendorId { get; set; }

        // NEW: Property to hold the Vendor's Name for GET responses
        public string? VendorName { get; set; } // Include vendor name for convenience
    }
}
